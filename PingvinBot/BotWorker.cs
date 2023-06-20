using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using PingvinBot.ChatGpt;

namespace PingvinBot;

public class BotWorker : IHostedService
{
    private readonly ILogger<BotWorker> _logger;
    private readonly DiscordClient _client;
    private readonly PingvinGptMessageHandler _messageHandler;

    public BotWorker(ILogger<BotWorker> logger, DiscordClient client, PingvinGptMessageHandler messageHandler)
    {
        _logger = logger;
        _client = client;
        _messageHandler = messageHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting bot");

        _client.SocketOpened += OnClientConnected;
        _client.SocketClosed += OnClientDisconnected;
        _client.Ready += OnClientReady;

        _client.MessageCreated += (_, args) => OnMessageCreated(args, cancellationToken);

        await _client.ConnectAsync();
    }

    private async Task OnMessageCreated(MessageCreateEventArgs args, CancellationToken cancellationToken)
    {
        var author = args.Author;

        if (author.IsBot || author.IsSystem.GetValueOrDefault())
        {
            return;
        }

        await _messageHandler.Handle(args, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping bot");

        await _client.DisconnectAsync();
    }

    private Task OnClientDisconnected(DiscordClient sender, SocketCloseEventArgs e)
    {
        _logger.LogInformation("Bot disconected: {Message}", e.CloseMessage);

        return Task.CompletedTask;
    }

    private Task OnClientConnected(DiscordClient sender, SocketEventArgs e)
    {
        _logger.LogInformation("Bot connected");

        return Task.CompletedTask;
    }

    private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        _logger.LogInformation("Bot ready");

        try
        {
            var activity = new DiscordActivity($"Chiller med gutta på Bouvetøya", ActivityType.Playing);

            await _client.UpdateStatusAsync(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OnClientReady");
        }
    }
}
