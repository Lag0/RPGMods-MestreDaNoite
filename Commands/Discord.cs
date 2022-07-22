using MDNMods.Utils;
using Wetstone.API;

namespace MDNMods.Commands;

[Command("discord", Usage = "discord", Description = "Send the Invite Link of the Discord Server!", ReqPermission = 0)]
public static class Discord
{
    public static string DiscordLink = "discord.gg/yourinvite";

    public static void Initialize(Context ctx)
    {
        if (ctx.Args.Length != 0)
            Output.InvalidArguments(ctx);
        else
            ctx.Event.User.SendSystemMessage($"Discord Invite Link: {DiscordLink}");
    }
}