using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("experience, exp, xp", Usage = "experience [<log> <on>|<off>]", Description = "Mostra sua experiência atual e progressão para o próximo nível, ou alterna a notificação de ganho de exp.")]
    public static class Experience
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;

            if (!ExperienceSystem.isEXPActive)
            {
                Output.CustomErrorMessage(ctx, "O sistema de experiência não está ativo.");
                return;
            }

            if (ctx.Args.Length >= 2 )
            {
                bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "experience_args");
                if (ctx.Args[0].Equals("set") && isAllowed && int.TryParse(ctx.Args[1], out int value))
                {
                    if (ctx.Args.Length == 3)
                    {
                        string name = ctx.Args[2];
                        if(Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                        {
                            CharName = name;
                            SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                            PlayerCharacter = targetEntity;
                            UserEntity = targetUserEntity;
                        }
                        else
                        {
                            Output.CustomErrorMessage(ctx, $"Não foi possível encontrar o jogador \"{name}\".");
                            return;
                        }
                    }
                    Database.player_experience[SteamID] = value;
                    ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
                    user.SendSystemMessage($"Player \"{CharName}\" Experiência setada para <color=#ffffffff> {ExperienceSystem.getXp(SteamID)}</color>");
                }
                else if (ctx.Args[0].ToLower().Equals("log"))
                {
                    if (ctx.Args[1].ToLower().Equals("on"))
                    {
                        Database.player_log_exp[SteamID] = true;
                        user.SendSystemMessage($"Ganho de experiência ativado.");
                        return;
                    }
                    else if (ctx.Args[1].ToLower().Equals("off"))
                    {
                        Database.player_log_exp[SteamID] = false;
                        user.SendSystemMessage($"Ganho de experiência desativado.");
                        return;
                    }
                }
                else
                {
                    Output.InvalidArguments(ctx);
                    return;
                }
            }
            else
            {
                int userLevel = ExperienceSystem.getLevel(SteamID);
                user.SendSystemMessage($"-- <color=#ffffffff>{CharName}</color> --");
                user.SendSystemMessage(
                    $"Level:<color=#ffffffff> {userLevel}</color> (<color=#ffffffff>{ExperienceSystem.getLevelProgress(SteamID)}%</color>) " +
                    $" [ XP:<color=#ffffffff> {ExperienceSystem.getXp(SteamID)}</color>/<color=#ffffffff>{ExperienceSystem.convertLevelToXp(userLevel + 1)}</color> ]");
            }
        }
    }
}
