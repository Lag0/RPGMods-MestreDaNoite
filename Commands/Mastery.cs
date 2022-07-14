using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("mastery, m", Usage = "mastery [<log> <on>|<off>]", Description = "Exiba sua progressão de maestria atual ou alterne a notificação.")]
    public static class Mastery
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                Output.CustomErrorMessage(ctx, "O sistema de maestria não está ativado.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            if (ctx.Args.Length > 1)
            {
                if (ctx.Args[0].ToLower().Equals("set") && ctx.Args.Length >= 3)
                {
                    bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "mastery_args");
                    if (!isAllowed) return;
                    if (int.TryParse(ctx.Args[2], out int value))
                    {
                        string CharName = ctx.Event.User.CharacterName.ToString();
                        var UserEntity = ctx.Event.SenderUserEntity;
                        var CharEntity = ctx.Event.SenderCharacterEntity;
                        if (ctx.Args.Length == 4)
                        {
                            string name = ctx.Args[3];
                            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                            {
                                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                                CharName = name;
                                UserEntity = targetUserEntity;
                                CharEntity = targetEntity;
                            }
                            else
                            {
                                Output.CustomErrorMessage(ctx, $"Não foi possível encontrar o jogador especificado \"{name}\".");
                                return;
                            }
                        }
                        string MasteryType = ctx.Args[1].ToLower();
                        if (MasteryType.Equals("sword")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Sword, value);
                        else if (MasteryType.Equals("none")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.None, value);
                        else if (MasteryType.Equals("spear")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Spear, value);
                        else if (MasteryType.Equals("crossbow")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Crossbow, value);
                        else if (MasteryType.Equals("slashers")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Slashers, value);
                        else if (MasteryType.Equals("scythe")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Scythe, value);
                        else if (MasteryType.Equals("fishingpole")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.FishingPole, value);
                        else if (MasteryType.Equals("mace")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Mace, value);
                        else if (MasteryType.Equals("axes")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Axes, value);
                        else 
                        {
                            Output.InvalidArguments(ctx);
                            return;
                        }
                        ctx.Event.User.SendSystemMessage($"{ctx.Args[1].ToUpper()} Maestria do \"{CharName}\" foi modificada para <color=#ffffffff>  {value * 0.001}%</color>");
                        Helper.ApplyBuff(UserEntity, CharEntity, Database.buff.Buff_VBlood_Perk_Moose);
                        return;
                        
                    }
                    else
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }
                if (ctx.Args[0].ToLower().Equals("log"))
                {
                    if (ctx.Args[1].ToLower().Equals("on"))
                    {
                        Database.player_log_mastery[SteamID] = true;
                        ctx.Event.User.SendSystemMessage($"O ganho de maestria agora é mostrado.");
                        return;
                    }
                    else if (ctx.Args[1].ToLower().Equals("off"))
                    {
                        Database.player_log_mastery[SteamID] = false;
                        ctx.Event.User.SendSystemMessage($"O ganho de maestria não está mais sendo mostrado.");
                        return;
                    }
                    else
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }
            }
            else
            {
                bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
                if (!isDataExist)
                {
                    Output.CustomErrorMessage(ctx, "Você nem tentou masterizar nada ainda...");
                    return;
                }

                ctx.Event.User.SendSystemMessage("------------------ <color=#ffffffff>Maestria das Armas</color> ------------------");
                ctx.Event.User.SendSystemMessage($"Espada:<color=#ffffffff> {(double)MasteryData.Sword * 0.001}%</color> (AD <color=#75FF33FF>10</color>, AP <color=#75FF33FF>10</color>, MOV <color=#75FF33FF>12.5%</color>)");
                ctx.Event.User.SendSystemMessage($"Lança:<color=#ffffffff> {(double)MasteryData.Spear * 0.001}%</color> (AD <color=#75FF33FF>25</color>)");
                ctx.Event.User.SendSystemMessage($"Machado:<color=#ffffffff> {(double)MasteryData.Axes * 0.001}%</color> (AD <color=#75FF33FF>15</color>, HP <color=#75FF33FF>50</color>)");
                ctx.Event.User.SendSystemMessage($"Foiçe:<color=#ffffffff> {(double)MasteryData.Scythe * 0.001}%</color> (AP <color=#75FF33FF>10</color>, CRIT <color=#75FF33FF>30%</color>, CD <color=#75FF33FF>25%</color>, HP <color=#F00000FF>100</color>)");
                ctx.Event.User.SendSystemMessage($"Adagas:<color=#ffffffff> {(double)MasteryData.Slashers * 0.001}%</color> (CRIT <color=#75FF33FF>100%</color>, MOV <color=#75FF33FF>18.75%</color>, HP <color=#F00000FF>50</color>)");
                ctx.Event.User.SendSystemMessage($"Maça:<color=#ffffffff> {(double)MasteryData.Mace * 0.001}%</color> (HP <color=#75FF33FF>100</color>, RESISTÊNCIA <color=#75FF33FF>5%</color>)");
                ctx.Event.User.SendSystemMessage($"Punho:<color=#ffffffff> {(double)MasteryData.None * 0.001}%</color> (AD <color=#75FF33FF>40</color>, AP <color=#75FF33FF>10</color>, MOV <color=#75FF33FF>12,5%</color>)");
                //ctx.Event.User.SendSystemMessage($"Punho Mágico:<color=#ffffffff> {(double)MasteryData.Spell * 0.001}%</color> (AP <color=#75FF33FF>10</color>)");
                ctx.Event.User.SendSystemMessage($"Besta:<color=#ffffffff> {(double)MasteryData.Crossbow * 0.001}%</color> (CRIT <color=#75FF33FF>30%</color>, AD <color=#75FF33FF>10</color>, ATKS <color=#75FF33FF>37.5%</color>)");
                //ctx.Event.User.SendSystemMessage($"Fishing Pole: <color=#ffffffff>{(double)MasteryData.FishingPole * 0.001}%</color> (??? ↑↑)");
            }
        }
    }
}
