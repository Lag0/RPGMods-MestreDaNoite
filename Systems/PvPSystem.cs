using ProjectM;
using ProjectM.Network;
using MDNMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;

namespace MDNMods.Systems
{
    public class PvPSystem
    {
        public static bool announce_kills = true;

        public static bool isLadderEnabled = true;
        public static bool isPvPToggleEnabled = true;
        public static bool isPunishEnabled = true;
        public static int PunishLevelDiff = -10;
        public static float PunishDuration = 1800f;
        public static int OffenseLimit = 3;
        public static float Offense_Cooldown = 300f;

        public static EntityManager em = VWorld.Server.EntityManager;

        private static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.FireResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.HolyResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SunResistance,
            Value = -15,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalPower,
            Value = 0.75f,
            ModificationType = ModificationType.Multiply,
            Id = ModificationId.NewId(0)
        };

        private static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellPower,
            Value = 0.75f,
            ModificationType = ModificationType.Multiply,
            Id = ModificationId.NewId(0)
        };

        public static void Monitor(Entity KillerEntity, Entity VictimEntity)
        {
            var killer = em.GetComponentData<PlayerCharacter>(KillerEntity);
            var killer_userEntity = killer.UserEntity._Entity;
            var killer_user = em.GetComponentData<User>(killer_userEntity);
            var killer_name = killer_user.CharacterName.ToString();
            var killer_id = killer_user.PlatformId;

            var victim = em.GetComponentData<PlayerCharacter>(VictimEntity);
            var victim_userEntity = victim.UserEntity._Entity;
            var victim_user = em.GetComponentData<User>(victim_userEntity);
            var victim_name = victim_user.CharacterName.ToString();
            var victim_id = victim_user.PlatformId;
            
            bool isAdminKiller = killer_user.IsAdmin;
            bool isAdminVictim = victim_user.IsAdmin;
            
            

            Database.pvpkills.TryGetValue(killer_id, out var KillerKills);
            Database.pvpdeath.TryGetValue(victim_id, out var VictimDeath);

            if(PermissionSystem.GetUserPermission(killer_id) < 50 && PermissionSystem.GetUserPermission(victim_id) < 50)
            {
                Database.pvpkills[killer_id] = KillerKills + 1;
                Database.pvpdeath[victim_id] = VictimDeath + 1;
            }
            
            //-- Update K/D
            UpdateKD(killer_id, victim_id);

            //-- Announce Kills only if enable and if the killer is not staff
            if (announce_kills)
            {

                if (PermissionSystem.GetUserPermission(killer_id) < 50 && PermissionSystem.GetUserPermission(victim_id) < 50)
                {
                    victim_user.SendSystemMessage($"Você foi morto por <color=#c90e21ff>\"{killer_name}\"</color>");
                    ServerChatUtils.SendSystemMessageToAllClients(em, $"<color=#47ff18>{killer_name}</color> empalou <color=#ff003e>{victim_name}</color>!");
                }
                    
            }
        }

        public static Dictionary<ulong, int> Kills = Database.pvpkills;
        public static Dictionary<ulong, int> Deaths = Database.pvpdeath;
        public static Dictionary<ulong, int> GetTopKillList() => Database.pvpkills
            .OrderBy<KeyValuePair<ulong, int>, int>((Func<KeyValuePair<ulong, int>, int>)(x => x.Value))
            .Reverse<KeyValuePair<ulong, int>>().ToDictionary<KeyValuePair<ulong, int>, ulong, int>(
                (Func<KeyValuePair<ulong, int>, ulong>)(x => x.Key), 
                (Func<KeyValuePair<ulong, int>, int>)(x => x.Value));
        public static double GetKDA(ulong killer_id)
        {
            Kills = Database.pvpkills;
            Deaths = Database.pvpdeath;
            double num1 = 0.0;
            double num2 = 0.0;
            if (Kills.ContainsKey(killer_id))
                num1 = (double) Kills[killer_id];
            if (Deaths.ContainsKey(killer_id))
                num2 = (double) Deaths[killer_id];
            return num2 == 0.0 ? num1 : num1 / num2;
        }
        
        public static void UpdateKD(ulong killer_id, ulong victim_id)
        {
            var isExist = Database.pvpdeath.TryGetValue(killer_id, out _);
            if (!isExist) Database.pvpdeath[killer_id] = 0;

            isExist = Database.pvpkills.TryGetValue(victim_id, out _);
            if (!isExist) Database.pvpkills[victim_id] = 0;

            if (Database.pvpdeath[killer_id] != 0) Database.pvpkd[killer_id] = (double)Database.pvpkills[killer_id] / Database.pvpdeath[killer_id];
            else Database.pvpkd[killer_id] = Database.pvpkills[killer_id];

            if (Database.pvpkills[victim_id] != 0) Database.pvpkd[victim_id] = (double)Database.pvpkills[victim_id] / Database.pvpdeath[victim_id];
            else Database.pvpkd[victim_id] = 0;
        }

        public static void PunishCheck(Entity Killer, Entity Victim)
        {
            Entity KillerUser = em.GetComponentData<PlayerCharacter>(Killer).UserEntity._Entity;
            ulong KillerSteamID = em.GetComponentData<User>(KillerUser).PlatformId;
            Equipment KillerGear = em.GetComponentData<Equipment>(Killer);
            float KillerLevel = KillerGear.ArmorLevel + KillerGear.WeaponLevel + KillerGear.SpellLevel;

            Equipment VictimGear = em.GetComponentData<Equipment>(Victim);
            float VictimLevel = VictimGear.ArmorLevel + VictimGear.WeaponLevel + VictimGear.SpellLevel;

            if (VictimLevel - KillerLevel <= PunishLevelDiff)
            {
                Cache.punish_killer_last_offense.TryGetValue(KillerSteamID, out var last_offense);
                TimeSpan timeSpan = DateTime.Now - last_offense;
                if (timeSpan.TotalSeconds > Offense_Cooldown) Cache.punish_killer_offense[KillerSteamID] = 1;
                else Cache.punish_killer_offense[KillerSteamID] += 1;
                Cache.punish_killer_last_offense[KillerSteamID] = DateTime.Now;

                if (Cache.punish_killer_offense[KillerSteamID] >= OffenseLimit)
                {
                    Helper.ApplyBuff(Killer, KillerUser, Database.buff.Severe_GarlicDebuff);
                }
            }
        }

        public static void BuffReceiver(Entity BuffEntity, PrefabGUID GUID)
        {
            if (GUID.Equals(Database.buff.Severe_GarlicDebuff))
            {
                var lifeTime_component = em.GetComponentData<LifeTime>(BuffEntity);
                lifeTime_component.Duration = PvPSystem.PunishDuration;
                em.SetComponentData(BuffEntity, lifeTime_component);

                var Buffer = em.AddBuffer<ModifyUnitStatBuff_DOTS>(BuffEntity);
                Buffer.Add(PPower);
                Buffer.Add(SPPower);
                Buffer.Add(HResist);
                Buffer.Add(FResist);
                Buffer.Add(SPResist);
                Buffer.Add(PResist);
            }
        }

        public static void SavePvPStat()
        {
            File.WriteAllText("BepInEx/config/MDNMods/Saves/pvpkills.json", JsonSerializer.Serialize(Database.pvpkills, Database.JSON_options));
            File.WriteAllText("BepInEx/config/MDNMods/Saves/pvpdeath.json", JsonSerializer.Serialize(Database.pvpdeath, Database.JSON_options));
            File.WriteAllText("BepInEx/config/MDNMods/Saves/pvpkd.json", JsonSerializer.Serialize(Database.pvpkd, Database.JSON_options));
        }

        public static void LoadPvPStat()
        {
            if (!File.Exists("BepInEx/config/MDNMods/Saves/pvpkills.json"))
            {
                var stream = File.Create("BepInEx/config/MDNMods/Saves/pvpkills.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/MDNMods/Saves/pvpkills.json");
            try
            {
                Database.pvpkills = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PvPKills DB Populated.");
            }
            catch
            {
                Database.pvpkills = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PvPKills DB Created.");
            }

            if (!File.Exists("BepInEx/config/MDNMods/Saves/pvpdeath.json"))
            {
                var stream = File.Create("BepInEx/config/MDNMods/Saves/pvpdeath.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/MDNMods/Saves/pvpdeath.json");
            try
            {
                Database.pvpdeath = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PvPDeath DB Populated.");
            }
            catch
            {
                Database.pvpdeath = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PvPDeath DB Created.");
            }

            if (!File.Exists("BepInEx/config/MDNMods/Saves/pvpkd.json"))
            {
                var stream = File.Create("BepInEx/config/MDNMods/Saves/pvpkd.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/MDNMods/Saves/pvpkd.json");
            try
            {
                Database.pvpkd = JsonSerializer.Deserialize<Dictionary<ulong, double>>(json);
                Plugin.Logger.LogWarning("PvPKD DB Populated.");
            }
            catch
            {
                Database.pvpkd = new Dictionary<ulong, double>();
                Plugin.Logger.LogWarning("PvPKD DB Created.");
            }
        }
    }
}
