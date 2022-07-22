using ProjectM.Network;
using MDNMods.Utils;
using Wetstone.API;

namespace MDNMods.Commands
{
    [Command("ping, p", Usage = "ping", Description = "Mostra sua latencia.")]
    public static class Ping
    {
        public static void Initialize(Context ctx)
        {
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            ctx.Event.User.SendSystemMessage($"Seu ping atual é <color=#ffff00ff>{ping*1000}</color>ms");
        }
    }
}
