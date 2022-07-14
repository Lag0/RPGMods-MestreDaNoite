using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("ping, p", Usage = "ping", Description = "Mostra sua latencia.")]
    public static class Ping
    {
        public static void Initialize(Context ctx)
        {
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            ctx.Event.User.SendSystemMessage($"Your latency is <color=#ffff00ff>{ping*1000}</color>ms");
        }
    }
}
