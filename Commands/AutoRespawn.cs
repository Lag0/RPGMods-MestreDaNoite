using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands;

[Command("autorespawn", Usage = "autorespawn [<PlayerName>]",
    Description = "Toggle auto respawn on the same position on death.")]
public static class AutoRespawn
{
    public static void Initialize(Context ctx)
    {
        var entityManager = ctx.EntityManager;
        var steamID = ctx.Event.User.PlatformId;
        var playerName = ctx.Event.User.CharacterName.ToString();
        var isServerWide = false;

        var isAllowed = ctx.Event.User.IsAdmin ||
                        PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "autorespawn_args");
        if (ctx.Args.Length > 0 && isAllowed)
        {
            var TargetName = string.Join(' ', ctx.Args);
            if (TargetName.ToLower().Equals("all"))
            {
                steamID = 1;
                isServerWide = true;
            }
            else
            {
                if (Helper.FindPlayer(TargetName, false, out var targetEntity, out var targetUserEntity))
                {
                    var user_component = entityManager.GetComponentData<User>(targetUserEntity);
                    steamID = user_component.PlatformId;
                    playerName = TargetName;
                }
                else
                {
                    Output.CustomErrorMessage(ctx, $"Player \"{TargetName}\" not found!");
                    return;
                }
            }
        }

        var isAutoRespawn = Database.autoRespawn.ContainsKey(steamID);
        if (isAutoRespawn) isAutoRespawn = false;
        else isAutoRespawn = true;
        UpdateAutoRespawn(steamID, isAutoRespawn);
        var s = isAutoRespawn ? "Activated" : "Deactivated";
        if (isServerWide)
            ctx.Event.User.SendSystemMessage($"Server wide Auto Respawn <color=#ffff00ff>{s}</color>");
        else
            ctx.Event.User.SendSystemMessage($"Player \"{playerName}\" Auto Respawn <color=#ffff00ff>{s}</color>");
    }

    public static bool UpdateAutoRespawn(ulong SteamID, bool isAutoRespawn)
    {
        var isExist = Database.autoRespawn.ContainsKey(SteamID);
        if (isExist || !isAutoRespawn) RemoveAutoRespawn(SteamID);
        else Database.autoRespawn.Add(SteamID, isAutoRespawn);
        return true;
    }

    public static void SaveAutoRespawn()
    {
        File.WriteAllText("BepInEx/config/RPGMods/Saves/autorespawn.json",
            JsonSerializer.Serialize(Database.autoRespawn, Database.JSON_options));
    }

    public static bool RemoveAutoRespawn(ulong SteamID)
    {
        if (Database.autoRespawn.ContainsKey(SteamID))
        {
            Database.autoRespawn.Remove(SteamID);
            return true;
        }

        return false;
    }

    public static void LoadAutoRespawn()
    {
        if (!File.Exists("BepInEx/config/RPGMods/Saves/autorespawn.json"))
        {
            var stream = File.Create("BepInEx/config/RPGMods/Saves/autorespawn.json");
            stream.Dispose();
        }

        var json = File.ReadAllText("BepInEx/config/RPGMods/Saves/autorespawn.json");
        try
        {
            Database.autoRespawn = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
            Plugin.Logger.LogWarning("AutoRespawn DB Populated.");
        }
        catch
        {
            Database.autoRespawn = new Dictionary<ulong, bool>();
            Plugin.Logger.LogWarning("AutoRespawn DB Created.");
        }
    }
}