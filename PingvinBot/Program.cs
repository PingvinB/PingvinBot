using DSharpPlus;
using PingvinBot;
using PingvinBot.ChatGpt;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<BotOptions>(hostContext.Configuration.GetSection(BotOptions.OptionsKey));

        services.AddHttpClient<ChatGptHttpClient>();

        services.AddSingleton<PingvinGptChannelCache>();
        services.AddSingleton<PingvinGptMessageHandler>();

        services.AddSingleton((_) =>
        {
            var defaultLogLevel = hostContext.Configuration.GetValue<string>("Logging:LogLevel:Default") ?? "Warning";
            var botToken = hostContext.Configuration.GetValue<string>("PingvinBot:BotToken");

            LogLevel logLevel = Enum.Parse<LogLevel>(defaultLogLevel);

            var discordConfig = new DiscordConfiguration
            {
                MinimumLogLevel = logLevel,
                TokenType = TokenType.Bot,
                Token = botToken,
                Intents = DiscordIntents.All,
            };

            return new DiscordClient(discordConfig);
        });

        services.AddHostedService<BotWorker>();
    })
    .Build();

host.Run();
