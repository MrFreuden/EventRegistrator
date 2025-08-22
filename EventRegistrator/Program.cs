using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Factories;
using EventRegistrator.Application.Handlers;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Infrastructure;
using EventRegistrator.Infrastructure.Config;
using EventRegistrator.Infrastructure.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace EventRegistrator
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_SHOWWINDOW = 0x0040;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static async Task Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (env == "Development")
                loggerConfig = loggerConfig.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting application (env: {Env})", env);

                var apiToken = Environment.GetEnvironmentVariable("API_TOKEN")
                    ?? throw new InvalidOperationException("API_TOKEN not set");

                var services = new ServiceCollection();

                services.AddLogging(b => b.ClearProviders().AddSerilog(dispose: true));

                var bot = new TelegramBotClient(apiToken);
                services.AddSingleton<ITelegramBotClient>(bot);

                RegisterAppServices(services);

                var sp = services.BuildServiceProvider();
                var messageHandler = sp.GetRequiredService<MessageHandler>();
                var callbackQueryHandler = sp.GetRequiredService<CallbackQueryHandler>();
                var botHandler = new BotHandler(messageHandler, callbackQueryHandler);

                if (env == "Development")
                {
                    await RunPolling(bot, botHandler);
                }
                else
                {
                    var webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL")
                        ?? throw new InvalidOperationException("WEBHOOK_URL not set");
                    await RunWebhook(bot, botHandler, webhookUrl);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void MoveConsoleToSecondMonitor()
        {
            var hWnd = GetConsoleWindow();
            int x = 400;
            int y = 1200;
            SetWindowPos(hWnd, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        private static async Task RunPolling(ITelegramBotClient bot, BotHandler handler)
        {
            MoveConsoleToSecondMonitor();
            using var cts = new CancellationTokenSource();
            Log.Information("Starting in polling mode...");
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cts.Token);
            Log.Information("Bot is running (polling). Press Ctrl+C to exit.");
            await WaitForShutdown(cts);
            Log.Information("Polling stopped.");
        }

        private static async Task RunWebhook(ITelegramBotClient bot, BotHandler handler, string webhookUrl)
        {
            using var cts = new CancellationTokenSource();
            Log.Information("Setting webhook to {Url}", webhookUrl);

            await bot.SetWebhook(webhookUrl);

            var listener = new HttpListener();
            var port = "8080";
            listener.Prefixes.Add($"http://+:{port}/");
            listener.Start();
            Log.Information("Listening HTTP on port {Port}", port);

            var httpTask = HandleHttp(listener, bot, handler, cts.Token);
            var shutdownTask = WaitForShutdown(cts);
            await Task.WhenAny(httpTask, shutdownTask);

            try { listener.Stop(); } catch { /* ignore */ }
            Log.Information("Webhook stopped.");
        }

        private static void RegisterAppServices(ServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
            var loader = new RepositoryLoader(EnvLoader.GetDataPath());
            var userRepository = loader.LoadData();
            //EnvLoader.LoadDefaultUser1(userRepository);
            //EnvLoader.LoadDefaultUser2(userRepository);
            //EnvLoader.LoadDefaultUser3(userRepository);
            //loader.SaveDataAsync(userRepository);
            //userRepository.Clear();
            services.AddSingleton(loader);
            services.AddSingleton<IUserRepository>(userRepository);
            services.AddSingleton(userRepository);
            services.AddSingleton<MessageSender>();
            services.AddSingleton<EventService>();
            services.AddSingleton<RegistrationService>();
            services.AddSingleton<ResponseManager>();
            services.AddSingleton<ICommandFactory, CommandStateFactory>();
            services.AddSingleton<IStateFactory, CommandStateFactory>();
            services.AddSingleton<IMenuStateFactory, MenuStateFactory>();
            services.AddSingleton<CommandStateFactory>();
            services.AddSingleton<PrivateMessageHandler>();
            services.AddSingleton<TargetChatMessageHandler>();
            services.AddSingleton<GeneralCallbackQueryHandler>();
            services.AddSingleton<UpdateRouter>(sp =>
                new UpdateRouter(
                    new IHandler[] {
                        sp.GetRequiredService<PrivateMessageHandler>(),
                        sp.GetRequiredService<TargetChatMessageHandler>()
                    },
                    new IHandler[] {
                        sp.GetRequiredService<GeneralCallbackQueryHandler>(),
                    },
                    sp.GetRequiredService<ILogger<UpdateRouter>>()
                ));
            services.AddSingleton<MessageHandler>();
            services.AddSingleton<CallbackQueryHandler>();
        }

        private static async Task HandleHttp(HttpListener listener, ITelegramBotClient bot, BotHandler handler, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext? ctx = null;
                try
                {
                    ctx = await listener.GetContextAsync();
                    if (ctx.Request.HttpMethod != "POST")
                    {
                        ctx.Response.StatusCode = 200;
                        await ctx.Response.OutputStream.FlushAsync();
                        ctx.Response.Close();
                        continue;
                    }

                    using var reader = new StreamReader(ctx.Request.InputStream);
                    var body = await reader.ReadToEndAsync();

                    // Логируем JSON, который пришёл
                    Log.Information("Received update JSON: {Body}", body);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    Update? update = null;
                    try
                    {
                        update = JsonSerializer.Deserialize<Update>(body, options);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to deserialize Update");
                    }

                    if (update != null)
                    {
                        await handler.HandleUpdateAsync(bot, update, token);
                    }
                    else
                    {
                        Log.Warning("Received POST without valid Update");
                    }

                    ctx.Response.StatusCode = 200;
                    await ctx.Response.OutputStream.FlushAsync();
                    ctx.Response.Close();
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    Log.Information("HttpListener stopped");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled error in HTTP handler");
                    if (ctx != null)
                    {
                        try
                        {
                            ctx.Response.StatusCode = 500;
                            await ctx.Response.OutputStream.FlushAsync();
                            ctx.Response.Close();
                        }
                        catch { /* ignore */ }
                    }
                }
            }
        }
        private static async Task WaitForShutdown(CancellationTokenSource cts)
        {
            EventHandler onExit = (_, _) => CancelTokenSafely(cts);
            ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; CancelTokenSafely(cts); };

            AppDomain.CurrentDomain.ProcessExit += onExit;
            Console.CancelKeyPress += onCancel;
            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException) { }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= onExit;
                Console.CancelKeyPress -= onCancel;
            }
        }

        private static void CancelTokenSafely(CancellationTokenSource cts)
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}
