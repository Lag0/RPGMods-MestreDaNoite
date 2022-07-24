using ProjectM.Network;
using MDNMods.Systems;
using MDNMods.Utils;
using Unity.Entities;
using Wetstone.API;

namespace MDNMods.Commands
{
    [Command("heat", Usage = "heat", Description = "Mostra seu nivel de procurado atual.")]
    public static class Heat
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;

            if (!HunterHunted.isActive)
            {
                Output.CustomErrorMessage(ctx, "Sistema de Procurado não está ativo.");
                return;
            }

            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "heat_args");
            if (ctx.Args.Length >= 2 && isAllowed)
            {
                string CharName = ctx.Event.User.CharacterName.ToString();
                if (ctx.Args.Length == 3)
                {
                    string name = ctx.Args[2];
                    if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                    {
                        SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                        CharName = name;
                        userEntity = targetUserEntity;
                        charEntity = targetEntity;
                    }
                    else
                    {
                        Output.CustomErrorMessage(ctx, $"Não foi possível encontrar o jogador especificado \"{name}\".");
                        return;
                    }
                }
                if (int.TryParse(ctx.Args[0], out var n)) Cache.heatlevel[SteamID] = n;
                if (int.TryParse(ctx.Args[1], out var nm)) Cache.bandit_heatlevel[SteamID] = nm;
                user.SendSystemMessage($"Jogador \"{CharName}\" valor de procurado alterado.");
                user.SendSystemMessage($"Humanos: <color=#ffff00ff>{Cache.heatlevel[SteamID]}</color> | Bandidos: <color=#ffff00ff>{Cache.bandit_heatlevel[SteamID]}</color>");
                HunterHunted.HeatManager(userEntity, charEntity, false);
                return;
            }

            HunterHunted.HeatManager(userEntity, charEntity, false);

            Cache.heatlevel.TryGetValue(SteamID, out var human_heatlevel);
            if (human_heatlevel >= 3000) Output.SendLore(userEntity,$"<color=#0048ffff>[Humanos]</color> <color=#c90e21ff>VOCÊ É UMA AMEAÇA...</color>");
            else if (human_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#0048ffff>[Humanos]</color> <color=#c90e21ff>Os caçadores de vampiros estão caçando você...</color>");
            else if (human_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#0048ffff>[Humanos]</color> <color=#c90e21ff>Esquadrões de elite estão caçando você...</color>");
            else if (human_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#0048ffff>[Humanos]</color> <color=#c4515cff>Soldados estão caçando você...</color>");
            else if (human_heatlevel >= 250) Output.SendLore(userEntity, $"<color=#0048ffff>[Humanos]</color> <color=#c9999eff>Os humanos estão caçando você...</color>");
            else Output.SendLore(userEntity, $"<color=#0048ffff>[Humanos]</color> <color=#ffffffff>Você não está sendo caçado...</color>");
            
            Cache.bandit_heatlevel.TryGetValue(SteamID, out var bandit_heatlevel);
            if (bandit_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandidos]</color> <color=#c90e21ff>Os bandidos realmente querem você morto...</color>");
            else if (bandit_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandidos]</color> <color=#c90e21ff>Um grande grupo de bandidos está caçando você...</color>");
            else if (bandit_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandidos]</color> <color=#c4515cff>Um pequeno grupo de bandidos está caçando você...</color>");
            else if (bandit_heatlevel >= 250) Output.SendLore(userEntity,$"<color=#ff0000ff>[Bandidos]</color> <color=#c9999eff>Os bandidos estão caçando você...</color>");
            else Output.SendLore(userEntity, $"<color=#ff0000ff>[Bandidos]</color> <color=#ffffffff>Os bandidos não estão te caçando...</color>");
            
            if (ctx.Args.Length == 1 && user.IsAdmin)
            {
                if (!ctx.Args[0].Equals("debug") && ctx.Args.Length != 2) return;
                user.SendSystemMessage($"Heat Cooldown: {HunterHunted.heat_cooldown}");
                user.SendSystemMessage($"Bandit Heat Cooldown: {HunterHunted.bandit_heat_cooldown}");
                user.SendSystemMessage($"Cooldown Interval: {HunterHunted.cooldown_timer}");
                user.SendSystemMessage($"Intervalo de Emboscada: {HunterHunted.ambush_interval}");
                user.SendSystemMessage($"Chance de Emboscada: {HunterHunted.ambush_chance}");
                user.SendSystemMessage($"Humanos: <color=#ffff00ff>{human_heatlevel}</color> | Bandidos: <color=#ffff00ff>{bandit_heatlevel}</color>");
            }
        }
    }
}
