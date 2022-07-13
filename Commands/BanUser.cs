using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using RPGMods.Systems;
using Wetstone.API;
using System.Linq;
using System;

namespace RPGMods.Commands
{
    [Command("ban", Usage = "ban <playername> <days> <reason>", Description = "Verifica a penalidade de um jogador específico ou aplique uma punição. 0 = permanente")]
    public static class BanUser
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;

            if (args.Length == 1)
            {
                if (Helper.FindPlayer(args[0], false, out _, out var targetUserEntity_))
                {
                    var targetData_ = VWorld.Server.EntityManager.GetComponentData<User>(targetUserEntity_);
                    if (BanSystem.IsUserBanned(targetData_.PlatformId, out var banData_))
                    {
                        TimeSpan duration = banData_.BanUntil - DateTime.Now;
                        ctx.Event.User.SendSystemMessage($"Player:<color=#ffffffff> {args[0]}</color>");
                        ctx.Event.User.SendSystemMessage($"Status:<color=#ffffffff> Banido</color> | Por:<color=#ffffffff> {banData_.BannedBy}</color>");
                        ctx.Event.User.SendSystemMessage($"Duração:<color=#ffffffff> {Math.Round(duration.TotalDays)}</color> dia(s) [<color=#ffffffff>{banData_.BanUntil}</color>]");
                        ctx.Event.User.SendSystemMessage($"Motivo:<color=#ffffffff> {banData_.Reason}</color>");
                        return;
                    }
                    else
                    {
                        Output.CustomErrorMessage(ctx, "O jogador especificado não está banido.");
                        return;
                    }
                }
                else
                {
                    Output.CustomErrorMessage(ctx, "Não foi possível encontrar o jogador especificado.");
                    return;
                }
            }

            if (args.Length < 3)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (!int.TryParse(args[1], out var days))
            {
                Output.InvalidArguments(ctx);
                return;
            }

            var name = args[0];
            var reason = string.Join(' ', args.Skip(2));
            if (reason.Length > 150)
            {
                Output.CustomErrorMessage(ctx, "Keep the reason short will ya?!");
                return;
            }

            if (Helper.FindPlayer(name, false, out _, out var targetUserEntity))
            {
                if(BanSystem.BanUser(ctx.Event.SenderUserEntity, targetUserEntity, days, reason, out var banData))
                {
                    var user = ctx.Event.User;
                    Helper.KickPlayer(targetUserEntity);
                    user.SendSystemMessage($"Player \"{name}\" foi Banido!.");
                    user.SendSystemMessage($"Banido até:<color=#ffffffff> {banData.BanUntil}</color>");
                    user.SendSystemMessage($"Motivop:<color=#ffffffff> {reason}</color>");
                    return;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"Falha ao banir o usuário \"{name}\".");
                    return;
                }
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Jogador especificado não encontrado.");
                return;
            }
        }
    }

    [Command("unban", Usage = "unban <playername>", Description = "Desbanir um player específico")]
    public static class UnbanUser
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            if (args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (Helper.FindPlayer(args[0], false, out _, out var targetUserEntity))
            {
                if (BanSystem.UnbanUser(targetUserEntity))
                {
                    ctx.Event.User.SendSystemMessage($"Player \"{args[0]}\" foi desbanido.");
                    return;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"O player específico não se encontra no banco de dados de banimentos");
                    return;
                }
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Player não encontrado.");
                return;
            }
        }
    }
}
