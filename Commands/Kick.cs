using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("kick", Usage = "kick <nomedojogador>", Description = "Expulse o jogador especificado para fora do servidor.")]
    public static class Kick
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            if (args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            var name = args[0];
            if (Helper.FindPlayer(name, true, out _, out var targetUserEntity))
            {
                Helper.KickPlayer(targetUserEntity);
                ctx.Event.User.SendSystemMessage($"Player \"{name}\" foi kickado do servidor.");
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Jogador não encontrado.");
            }
        }
    }
}
