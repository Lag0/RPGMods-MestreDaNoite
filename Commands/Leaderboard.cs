using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("leaderboard, lb", Usage = "leaderboard / lb ",
        Description = "Show the current leaderboard ranking.", ReqPermission = 0)]
    public static class Leaderboard
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            Dictionary<ulong, int> topKillList = PvPSystem.GetTopKillList();

            user.SendSystemMessage("==========<color=#ffffffff>TOP PvP Players</color>==========");
            for (int index = 0; index < 10; ++index)
            {
                if (topKillList.Count >= index + 1)
                {
                    KeyValuePair<ulong, int> keyValuePair = topKillList.ElementAt<KeyValuePair<ulong, int>>(index);
                    string name = Helper.GetNameFromSteamID(keyValuePair.Key);
                    keyValuePair = topKillList.ElementAt<KeyValuePair<ulong, int>>(index);
                    var value = (ValueType)keyValuePair.Value;
                    keyValuePair = topKillList.ElementAt<KeyValuePair<ulong, int>>(index);
                    string str = PvPSystem.GetKDA(keyValuePair.Key).ToString("0.0");
                    user.SendSystemMessage(
                        $"{index + 1}. <color=#ffffff><b>{(object)name}:</b></color> <color=#75FF33FF>{(object)value}</color> Kills・<color=#ffffff>{(object)str}</color> KDA");
                }
            }

            user.SendSystemMessage("===================================");
        }
    }

    [Command("status, stats", Usage = "status / stats", Description = "Display your personal status.", ReqPermission = 0)]
    public static class Stats
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var charName = user.CharacterName.ToString();
            var steamID = user.PlatformId;

            Database.pvpkills.TryGetValue(steamID, out var pvpKills);
            Database.pvpdeath.TryGetValue(steamID, out var pvpDeaths);
            Database.pvpkd.TryGetValue(steamID, out var pvpKd);

            user.SendSystemMessage($"<b>-- <color=#FFFFFFFF>{charName}</color> --</b>");
            user.SendSystemMessage($"K/D: <color=#FFFFFFFF>{pvpKd:0.0}</color>");
            user.SendSystemMessage($"Kills: <color=#75FF33FF>{pvpKills}</color>");
            user.SendSystemMessage($"Deaths: <color=#F00000FF>{pvpDeaths}</color>");
            user.SendSystemMessage(
                $"You are No. <color=#FFFFFFFF><b>{(PvPSystem.GetTopKillList().Keys.ToList<ulong>().IndexOf(ctx.Event.User.PlatformId) + 1)}</b></color> in the leaderboard!");
        }
    }
}