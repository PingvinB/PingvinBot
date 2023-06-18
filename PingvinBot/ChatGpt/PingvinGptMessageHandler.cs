using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;
using PingvinBot.Utils;
using TiktokenSharp;

namespace PingvinBot.ChatGpt;

public class PingvinGptMessageHandler
{
    private const string ChatGptModel = "gpt-3.5-turbo";
    private const string MetaMessagePrefix = "msg";
    private const float Temperature = 1.2f;
    private const int MaxTokens = 4096;
    private const int MaxTokensToGenerate = 512; // Roughly the limit of a Discord message

    private readonly ILogger<PingvinGptMessageHandler> _logger;
    private readonly ChatGptHttpClient _chatGpt;
    private readonly BotOptions _botOptions;
    private readonly PingvinGptChannelCache _cache;
    private readonly TikToken _tokenizer;

    public PingvinGptMessageHandler(
        ILogger<PingvinGptMessageHandler> logger,
        ChatGptHttpClient chatGpt,
        IOptions<BotOptions> botOptions,
        PingvinGptChannelCache cache)
    {
        _logger = logger;
        _chatGpt = chatGpt;
        _botOptions = botOptions.Value;
        _cache = cache;

        _tokenizer = TikToken.EncodingForModel(ChatGptModel);
    }

    public async Task Handle(MessageCreateEventArgs args, CancellationToken cancellationToken)
    {
        var message = args.Message;
        var author = args.Author;
        var channel = args.Message.Channel;

        if (!ShouldHandleMessage(message))
        {
            return;
        }

        await channel.TriggerTypingAsync();

        try
        {
            var systemPromptsMessages = BuildSystemPromptsMessages(channel);

            var boundedMessageQueue = GetBoundedMessageQueue(channel, systemPromptsMessages);

            // Add new message from notification
            var newMessageContent = message.Content;
            var newMessageUser = author.GetNicknameOrUsername();

            var newUserMessage = new ChatCompletionMessage { Role = "user", Content = $"{newMessageUser}: {newMessageContent}" };

            boundedMessageQueue.Enqueue(newUserMessage, _tokenizer.Encode(newUserMessage.Content).Count);

            // Collect request messages
            var requestMessages = new List<ChatCompletionMessage>();
            requestMessages.AddRange(systemPromptsMessages);
            requestMessages.AddRange(boundedMessageQueue.GetAll());

            // Make request
            var request = new ChatCompletionCreateRequest()
            {
                Model = ChatGptModel,
                Messages = requestMessages.ToArray(),
                User = author.Id.ToString(),
                MaxTokens = MaxTokensToGenerate,
                Temperature = Temperature,
            };

            var response = await _chatGpt.ChatCompletionCreate(request, cancellationToken);

            var responseMessage = response.Choices[0].Message;

            // Reply to user
            await message.RespondAsync(responseMessage.Content);

            // Add the chat gpt response message to the bounded queue
            boundedMessageQueue.Enqueue(responseMessage, _tokenizer.Encode(responseMessage.Content).Count);

            SaveBoundedMessageQueue(channel, boundedMessageQueue);
        }
        catch (Exception ex)
        {
            await channel.SendMessageAsync($"*Error: {ex.Message}*");
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }

    private List<ChatCompletionMessage> BuildSystemPromptsMessages(DiscordChannel channel)
    {
        // Get core system prompt messages
        var coreSystemPrompts = string.Join(" ", _botOptions.CoreSystemPrompts);
        var systemPromptsMessages = new List<ChatCompletionMessage>() { new ChatCompletionMessage { Role = "system", Content = coreSystemPrompts } };

        // Get channel system prompts if they exist
        string[] channelPromptsArray = _botOptions.ChannelConfig.Where(x => x.Name.Equals(channel.Name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault()?.SystemPrompts ?? Array.Empty<string>();
        if (channelPromptsArray.Length > 0)
        {
            string channelSystemPrompts = string.Join(" ", channelPromptsArray);

            systemPromptsMessages.Add(new ChatCompletionMessage { Role = "system", Content = channelSystemPrompts });
        }

        return systemPromptsMessages;
    }

    private BoundedQueue<ChatCompletionMessage> GetBoundedMessageQueue(DiscordChannel channel, List<ChatCompletionMessage> systemPromptsMessages)
    {
        var cacheKey = PingvinGptChannelCache.PingvinGptChannelCacheKey(channel.Id);
        var boundedMessageQueue = _cache.GetCache(cacheKey);
        if (boundedMessageQueue == null)
        {
            var totalTokenCountForSystemMessages = systemPromptsMessages.Select(x => x.Content).Sum(m => _tokenizer.Encode(m).Count);

            var remainingTokensForContextMessages = MaxTokens - totalTokenCountForSystemMessages;

            boundedMessageQueue = new BoundedQueue<ChatCompletionMessage>(remainingTokensForContextMessages);
        }

        return boundedMessageQueue;
    }

    private void SaveBoundedMessageQueue(DiscordChannel channel, BoundedQueue<ChatCompletionMessage> boundedMessageQueue)
    {
        var cacheKey = PingvinGptChannelCache.PingvinGptChannelCacheKey(channel.Id);
        _cache.SetCache(cacheKey, boundedMessageQueue);
    }

    private bool ShouldHandleMessage(DiscordMessage message)
    {
        var messageIsReplyToPingvin = message.ReferencedMessage?.Author?.IsCurrent ?? false;

        if (messageIsReplyToPingvin)
        {
            return true;
        }

        var pingvinIsMentioned = message.MentionedUsers.Any(u => u.IsCurrent);

        if (pingvinIsMentioned)
        {
            return true;
        }

        var chattyPingvinChannel = _botOptions.ChannelConfig
                                    .Where(x => x.Name.Equals(message.Channel.Name, StringComparison.InvariantCultureIgnoreCase))
                                    .SingleOrDefault();

        if (chattyPingvinChannel?.ChattyPingvin ?? false)
        {
            var isMetaMessage = message.Content.StartsWith(MetaMessagePrefix, StringComparison.OrdinalIgnoreCase);

            return !isMetaMessage;
        }

        return false;
    }
}
