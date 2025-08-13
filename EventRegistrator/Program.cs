using DotNetEnv;
using EventRegistrator.Application;
using EventRegistrator.Application.Handlers;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Telegram.Bot;

namespace EventRegistrator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var apiToken = GetApiToken();
            Console.WriteLine("Getting api token");
            await Run(apiToken);
        }

        private static string GetApiToken()
        {
            DotNetEnv.Env.Load();
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
            if (apiToken == null) throw new ArgumentNullException(apiToken);
            return apiToken;
        }

        private static async Task Run(string apiToken)
        {
            using var cts = new CancellationTokenSource();
            var listener = ConfigureHttpListener();
            var bot = ConfigureBot(apiToken, cts);

            var httpTask = HandleHttpRequestsAsync(listener, cts);
            var shutdownTask = HandleShutdownAsync(cts);

            try
            {
                await Task.WhenAny(httpTask, shutdownTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Application is shutting down...");
            }
            finally
            {
                StopServices(listener, cts);
            }
            Console.WriteLine("All services stopped.");
        }

        private static HttpListener ConfigureHttpListener()
        {
            var listener = new HttpListener();
            var port = "8080";
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            Console.WriteLine($"Listening on port {port}...");
            return listener;
        }

        private static TelegramBotClient ConfigureBot(string apiToken, CancellationTokenSource cancellationToken)
        {
            var services = new ServiceCollection();
            
            var bot = new TelegramBotClient(apiToken); 

            services.AddSingleton<ITelegramBotClient>(bot);
            
            DI(services);

            var serviceProvider = services.BuildServiceProvider();

            //EnvLoader.LoadDefaultUser1(userRepository);
            //loader.SaveDataAsync(userRepository);
            //EnvLoader.LoadDefaultUser2(userRepository);
            //EnvLoader.LoadDefaultUser3(userRepository);
            //userRepository.Clear();
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

        private static async Task HandleHttpRequestsAsync(HttpListener listener, CancellationTokenSource cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    Console.WriteLine($"Received HTTP request: {context.Request.RawUrl}");
                    context.Response.StatusCode = 200;
                    await using var writer = new StreamWriter(context.Response.OutputStream);
                    await writer.WriteAsync("OK");
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    Console.WriteLine("Listener stopped.");
                }
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

        private static void StopServices(HttpListener listener, CancellationTokenSource cts)
        {
            Console.WriteLine("Stopping services...");
            CancelTokenSafely(cts);
            listener.Stop();
        }

        private static void CancelTokenSafely(CancellationTokenSource cts)
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}
