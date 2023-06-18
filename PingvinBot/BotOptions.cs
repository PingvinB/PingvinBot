namespace PingvinBot;

public record BotOptions
{
    public const string OptionsKey = "PingvinBot";

    public string BotToken { get; set; } = null!;

    public string OpenAiApiKey { get; set; } = null!;

    public string[] CoreSystemPrompts { get; set; } = Array.Empty<string>();

    public ChannelOptions[] ChannelConfig { get; set; } = Array.Empty<ChannelOptions>();
}

public record ChannelOptions
{
    public string Name { get; set; } = null!;

    public string[] SystemPrompts { get; set; } = Array.Empty<string>();

    public bool ChattyPingvin { get; set; } = false;
}
