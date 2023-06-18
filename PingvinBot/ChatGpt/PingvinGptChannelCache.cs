using PingvinBot.Utils;

namespace PingvinBot.ChatGpt;

public class PingvinGptChannelCache : SimpleMemoryCache
{
    private const int CacheSize = 128;

    private const int AbsoluteCacheDurationInDays = 7;
    private const int SlidingCacheDurationInHours = 1;

    public PingvinGptChannelCache()
        : base(CacheSize)
    {
    }

    public static string PingvinGptChannelCacheKey(ulong channelId) => $"{nameof(PingvinGptChannelCache)}_{channelId}";

    public BoundedQueue<ChatCompletionMessage>? GetCache(string key)
    {
        return GetCache<BoundedQueue<ChatCompletionMessage>>(key);
    }

    public void SetCache(string key, BoundedQueue<ChatCompletionMessage> value)
    {
        var absoluteExpirationRelativeToNow = TimeSpan.FromDays(AbsoluteCacheDurationInDays);
        var slidingExpiration = TimeSpan.FromHours(SlidingCacheDurationInHours);

        SetCache(key, value, absoluteExpirationRelativeToNow, slidingExpiration);
    }
}
