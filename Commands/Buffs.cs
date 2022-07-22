using ProjectM;
using MDNMods.Utils;
using Wetstone.API;

namespace MDNMods.Commands
{
    [Command("buff", Usage = "buff <Type> [<PlayerName>]", Description = "Sets your current buff")]
    public static class ShardBuff
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                try
                {
                    PrefabGUID type = new PrefabGUID();

                    if (ctx.Args.Length >= 1) type = Helper.GetBuffFromName(ctx.Args[0]);
                    
                    Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, type);
                    ctx.Event.User.SendSystemMessage($"<color=#ffff00ff>Buff Setado!!</color>");
                }
                catch
                {
                    Output.InvalidArguments(ctx);
                }
            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}