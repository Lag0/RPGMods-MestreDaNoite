using System;
using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
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
                    
                    ctx.Event.User.SendSystemMessage($"<color=#ffff00ff>Buff Setado!!</color>");
                    Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, type);
                }
                catch
                {
                    Utils.Output.InvalidArguments(ctx);
                }
            }
            else
            {
                Utils.Output.MissingArguments(ctx);
            }
        }
    }
}