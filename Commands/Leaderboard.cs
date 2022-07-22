using MDNMods.Systems;
using MDNMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Wetstone.API;

namespace MDNMods.Commands
{
    [Command("leaderboard, lb, pvp", Usage = "leaderboard / lb / pvp ",
        Description = "Mostra o rank atual!", ReqPermission = 0)]
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

    [Command("status, stats", Usage = "status / stats", Description = "Mostra suas estáticas pessoais", ReqPermission = 0)]
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
            user.SendSystemMessage($"Abates: <color=#75FF33FF>{pvpKills}</color>");
            user.SendSystemMessage($"Mortes: <color=#F00000FF>{pvpDeaths}</color>");
            user.SendSystemMessage(
                $"Você é o numero <color=#FFFFFFFF><b>{(PvPSystem.GetTopKillList().Keys.ToList<ulong>().IndexOf(ctx.Event.User.PlatformId) + 1)}</b></color> do rank!");
        }
    }
}