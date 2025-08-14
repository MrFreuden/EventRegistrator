using EventRegistrator.Application;
using EventRegistrator.Application.Handlers;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace EventRegistrator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            DotNetEnv.Env.Load();
            var apiToken = Environment.GetEnvironmentVariable("API_TOKEN")
                ?? throw new InvalidOperationException("API_TOKEN not set");

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            if (env == "Development")
            {
                Console.WriteLine("Starting in polling mode...");
                await RunPolling(apiToken);
            }
            else
            {
                Console.WriteLine("Starting in webhook mode...");
                var webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL")
                    ?? throw new InvalidOperationException("WEBHOOK_URL not set");
                await RunWebhook(apiToken, webhookUrl);
            }
        }

        private static async Task RunPolling(string apiToken)
        {
            using var cts = new CancellationTokenSource();
            var bot = ConfigureBot(apiToken, cts);

            Console.WriteLine("Bot running in polling mode. Press Ctrl+C to exit.");
            await HandleShutdownAsync(cts);
        }

        private static async Task RunWebhook(string apiToken, string webhookUrl)
        {
            using var cts = new CancellationTokenSource();
            var bot = ConfigureBot(apiToken, cts);

            await bot.SetWebhook(webhookUrl);

            var listener = new HttpListener();
            var port = "8080";
            listener.Prefixes.Add($"http://+:{port}/");
            listener.Start();
            Console.WriteLine($"Listening on port {port}...");

            var httpTask = HandleHttpRequestsAsync(listener, cts, bot);
            var shutdownTask = HandleShutdownAsync(cts);
            await Task.WhenAny(httpTask, shutdownTask);

            listener.Stop();
        }

        private static TelegramBotClient ConfigureBot(string apiToken, CancellationTokenSource cancellationToken)
        {
            var services = new ServiceCollection();

            var bot = new TelegramBotClient(apiToken);

            services.AddSingleton<ITelegramBotClient>(bot);

            DI(services);

            var serviceProvider = services.BuildServiceProvider();

            
            var messageHandler = serviceProvider.GetRequiredService<MessageHandler>();
            var callbackQueryHandler = serviceProvider.GetRequiredService<CallbackQueryHandler>();

            var handler = new BotHandler(messageHandler, callbackQueryHandler);
            Console.WriteLine("Starting bot...");
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cancellationToken.Token);
            Console.WriteLine("Bot is running.");
            return bot;
        }

        private static void DI(ServiceCollection services)
        {
            var loader = new RepositoryLoader(EnvLoader.GetDataPath());
            var userRepository = loader.LoadData();
            //EnvLoader.LoadDefaultUser1(userRepository);
            //loader.SaveDataAsync(userRepository);
            //EnvLoader.LoadDefaultUser2(userRepository);
            //EnvLoader.LoadDefaultUser3(userRepository);
            //userRepository.Clear();

            services.AddSingleton(loader);
            services.AddSingleton<IUserRepository>(userRepository);
            services.AddSingleton(userRepository);

            services.AddSingleton<MessageSender>();
            services.AddSingleton<EventService>();
            services.AddSingleton<RegistrationService>();
            services.AddSingleton<ResponseManager>();
            services.AddSingleton<ICommandFactory, CommandStateFactory>();
            services.AddSingleton<PrivateMessageHandler>();
            services.AddSingleton<TargetChatMessageHandler>();
            services.AddSingleton<UpdateRouter>(sp =>
                new UpdateRouter(new IHandler[] {
                    sp.GetRequiredService<PrivateMessageHandler>(),
                    sp.GetRequiredService<TargetChatMessageHandler>()
                }));
            services.AddSingleton<MessageHandler>();
            services.AddSingleton<CallbackQueryHandler>();
        }

        private static async Task HandleHttpRequestsAsync(HttpListener listener, CancellationTokenSource cancellationToken, ITelegramBotClient bot)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(context.Request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    var update = JsonSerializer.Deserialize<Update>(body);

                    if (update != null)
                    {
                        var services = new ServiceCollection();
                        services.AddSingleton(bot);
                        DI(services);
                        var sp = services.BuildServiceProvider();

                        var messageHandler = sp.GetRequiredService<MessageHandler>();
                        var callbackQueryHandler = sp.GetRequiredService<CallbackQueryHandler>();
                        var handler = new BotHandler(messageHandler, callbackQueryHandler);

                        await handler.HandleUpdateAsync(bot, update, cancellationToken.Token);
                    }
                }
                context.Response.StatusCode = 200;
                await context.Response.OutputStream.FlushAsync();
                context.Response.Close();
            }
        }

        private static async Task HandleShutdownAsync(CancellationTokenSource cts)
        {
            EventHandler processExitHandler = (_, _) => CancelTokenSafely(cts);
            ConsoleCancelEventHandler cancelKeyPressHandler = (_, e) =>
            {
                e.Cancel = true;
                CancelTokenSafely(cts);
            };

            AppDomain.CurrentDomain.ProcessExit += processExitHandler;
            Console.CancelKeyPress += cancelKeyPressHandler;
            try
            {
                await Task.Delay(-1, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                Console.CancelKeyPress -= cancelKeyPressHandler;
            }
        }

        private static void CancelTokenSafely(CancellationTokenSource cts)
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}
