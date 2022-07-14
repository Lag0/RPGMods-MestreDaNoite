using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("pvp", Usage = "pvp [<on|off>]", Description = "Exibir líderes atuais do rank.")]
    public static class PvP
    {
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            if (ctx.Args.Length == 0)
            {
                if (PvPSystem.isLadderEnabled)
                {
                    List<KeyValuePair<ulong, int>> list1 = Database.pvpkills.ToList<KeyValuePair<ulong, int>>();
                    list1.Sort((Comparison<KeyValuePair<ulong, int>>)((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)));
                    list1.Take<KeyValuePair<ulong, int>>(5);
                    user.SendSystemMessage("==========<color=#ff0000ff>TOP 5 PvP Players</color>==========");
                    int num4 = 0;
                    foreach (KeyValuePair<ulong, int> keyValuePair in list1.Take<KeyValuePair<ulong, int>>(5))
                    {
                        ++num4;
                        user.SendSystemMessage(string.Format("{0}. <color=#ffffffff>{1}: </color> <color=#ff0000ff>{2} Kills</color>", (object)num4, (object)Helper.GetNameFromSteamID(keyValuePair.Key), (object)keyValuePair.Value));
                    }
                    if (num4 == 0)
                        user.SendSystemMessage("<color=#ffffffff>Sem Resultado</color>");
                    List<KeyValuePair<ulong, double>> list2 = Database.pvpkd.ToList<KeyValuePair<ulong, double>>();
                    list2.Sort((Comparison<KeyValuePair<ulong, double>>)((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)));
                    list2.Take<KeyValuePair<ulong, double>>(5);
                    user.SendSystemMessage("==========<color=#ff0000ff>TOP 5 KD Players</color>==========");
                    int num5 = 0;
                    foreach (KeyValuePair<ulong, double> keyValuePair in list2.Take<KeyValuePair<ulong, double>>(5))
                    {
                        ++num5;
                        user.SendSystemMessage(string.Format("{0}. <color=#ffffffff>{1}: </color> <color=#ff0000ff>{2} KD</color>", (object)num5, (object)Helper.GetNameFromSteamID(keyValuePair.Key), (object)keyValuePair.Value.ToString("F")));
                    }
                    if (num5 == 0)
                        user.SendSystemMessage("<color=#ffffffff>Sem Resultado</color>");
                        user.SendSystemMessage("===================================");
                }
            }
            
            if (ctx.Args.Length > 0)
            {
                var isPvPShieldON = false;
                if (ctx.Args[0].ToLower().Equals("on")) isPvPShieldON = false;
                else if (ctx.Args[0].ToLower().Equals("off")) isPvPShieldON = true;
                else
                {
                    Utils.Output.InvalidArguments(ctx);
                    return;
                }

                if (ctx.Args.Length == 1)
                {
                    if (!PvPSystem.isPvPToggleEnabled)
                    {
                        Output.CustomErrorMessage(ctx, "PvP toggling is not enabled!");
                        return;
                    }
                    if (Helper.IsPlayerInCombat(charEntity))
                    {
                        Output.CustomErrorMessage(ctx, $"Unable to change PvP Toggle, you are in combat!");
                        return;
                    }
                    Helper.SetPvPShield(charEntity, isPvPShieldON);
                    string s = isPvPShieldON ? "OFF" : "ON";
                    user.SendSystemMessage($"PvP is now {s}");
                    return;
                }
                else if (ctx.Args.Length == 2 && (ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "pvp_args")))
                {
                    try
                    {
                        string name = ctx.Args[2];
                        if (Helper.FindPlayer(name,false,out Entity targetChar, out Entity targetUser))
                        {
                            Helper.SetPvPShield(targetChar, isPvPShieldON);
                            string s = isPvPShieldON ? "OFF" : "ON";
                            user.SendSystemMessage($"Player \"{name}\" PvP is now {s}");
                        }
                        else
                        {
                            Output.CustomErrorMessage(ctx, $"Unable to find the specified player!");
                        }
                    }
                    catch
                    {
                        Output.InvalidArguments(ctx);
                    }
                }
            }
        }
    }
}
