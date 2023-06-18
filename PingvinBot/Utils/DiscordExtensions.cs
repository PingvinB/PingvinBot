using DSharpPlus.Entities;

namespace PingvinBot.Utils;

public static class DiscordExtensions
{
    public static string GetNicknameOrUsername(this DiscordUser user)
    {
        string username = user.Username;

        if (user is DiscordMember member)
        {
            username = !string.IsNullOrWhiteSpace(member.Nickname)
                ? member.Nickname
                : member.DisplayName;
        }

        return username;
    }
}
