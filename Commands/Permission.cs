using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System.Linq;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("permission, perm", Usage = "permission <list>|<save>|<reload>|<set> <0-100> <playername>|<steamid>", Description = "Gerenciar comandos e nível de permissões do usuário.")]
    public static class Permission
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;

            if (args.Length == 1)
            {
                if (args[0].ToLower().Equals("list"))
                {
                    var SortedPermission = Database.user_permission.ToList();
                    SortedPermission.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                    var ListPermission = SortedPermission;
                    ctx.Event.User.SendSystemMessage($"===================================");
                    int i = 0;
                    foreach (var result in ListPermission)
                    {
                        i++;
                        ctx.Event.User.SendSystemMessage($"{i}. <color=#ffffffff>{Helper.GetNameFromSteamID(result.Key)} : {result.Value}</color>");
                    }
                    if (i == 0) ctx.Event.User.SendSystemMessage($"<color=#ffffffff>No Result</color>");
                    ctx.Event.User.SendSystemMessage($"===================================");
                }
                else if (args[0].ToLower().Equals("save"))
                {
                    PermissionSystem.SaveUserPermission();
                    ctx.Event.User.SendSystemMessage("Permissão de usuário salva no arquivo JSON.");
                }
                else if (args[0].ToLower().Equals("reload"))
                {
                    PermissionSystem.LoadPermissions();
                    ctx.Event.User.SendSystemMessage("Permissão recarregada do arquivo JSON.");
                }
                else
                {
                    Output.MissingArguments(ctx);
                }
                return;
            }

            if (args.Length < 3)
            {
                Output.MissingArguments(ctx);
                return;
            }

            if (args[0].ToLower().Equals("set")) {
                var tryParse = int.TryParse(args[1], out var level);
                if (!tryParse)
                {
                    Output.InvalidArguments(ctx);
                    return;
                }
                else
                {
                    if (level < 0 || level > 100)
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }

                var tryParse_2 = ulong.TryParse(args[2], out var SteamID);
                string playerName = null;
                if (!tryParse_2)
                {
                    bool tryFind = Helper.FindPlayer(args[2], false, out var target_playerEntity, out var target_userEntity);
                    if (!tryFind)
                    {
                        Output.CustomErrorMessage(ctx, $"Não foi possível encontrar o jogador especificado\"{args[2]}\".");
                        return;
                    }
                    playerName = args[2];
                    SteamID = VWorld.Server.EntityManager.GetComponentData<User>(target_userEntity).PlatformId;
                }
                else
                {
                    playerName = Helper.GetNameFromSteamID(SteamID);
                    if (playerName == null)
                    {
                        Output.CustomErrorMessage(ctx, $"Não foi possível encontrar o ID do Steam do jogador especificado \"{args[2]}\".");
                        return;
                    }
                }

                if (level == 0) Database.user_permission.Remove(SteamID);
                else Database.user_permission[SteamID] = level;

                ctx.Event.User.SendSystemMessage($"Player \"{playerName}\" Permissão setada para<color=#ffffffff> {level}</color>.");
                return;
            }
            else
            {
                Output.InvalidArguments(ctx);
                return;
            }
        }
    }
}
