/*using ProjectM;
using RPGMods.Utils;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("inventory, i", "Inv Clear", "Limpa tudo, exceto armas e roupas equipadas do inventário.")]
    public static class InventoryClear
    {
        public static void Initialize(Context ctx)
        {
            Entity senderCharacterEntity = ctx.Event.SenderCharacterEntity;
            Entity entity = ctx.EntityManager;
            InventoryUtilities.TryGetInventoryEntity(ctx.EntityManager, senderCharacterEntity, ref entity);
            for (int index = 9; index < InventoryUtilities.GetItemSlots(ctx.EntityManager, entity); ++index)
                InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, entity, index);
            ctx.Event.User.SendSystemMessage("<color=#ffff00ff>Inventario Limpo!</color>");
        }
    }
}*/