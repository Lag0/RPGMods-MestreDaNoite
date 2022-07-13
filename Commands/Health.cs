using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("health, hp", Usage = "health <porcentagem> [<nome do player>]", Description = "Define a vida atual sua ou de outro player")]
    public static class Health
    {
        public static void Initialize(Context ctx)
        {
            var PlayerName = ctx.Event.User.CharacterName;
            var UserIndex = ctx.Event.User.Index;
            var component = ctx.EntityManager.GetComponentData<ProjectM.Health>(ctx.Event.SenderCharacterEntity);
            int Value = 100;
            if (ctx.Args.Length != 0)
            {
                if (!int.TryParse(ctx.Args[0], out Value))
                {
                    Utils.Output.InvalidArguments(ctx);
                    return;
                }
                else Value = int.Parse(ctx.Args[0]);
            }

            if (ctx.Args.Length == 2)
            {
                var targetName = ctx.Args[1];
                if (Helper.FindPlayer(targetName, true, out var targetEntity, out var targetUserEntity))
                {
                    PlayerName = targetName;
                    UserIndex = VWorld.Server.EntityManager.GetComponentData<User>(targetUserEntity).Index;
                    component = VWorld.Server.EntityManager.GetComponentData<ProjectM.Health>(targetEntity);
                }
                else
                {
                    Utils.Output.CustomErrorMessage(ctx, $"Player \"{targetName}\" não encontrada.");
                    return;
                }
            }

            float restore_hp = ((component.MaxHealth / 100) * Value) - component.Value;

            var HealthEvent = new ChangeHealthDebugEvent()
            {
                Amount = (int)restore_hp
            };
            VWorld.Server.GetExistingSystem<DebugEventsSystem>().ChangeHealthEvent(UserIndex, ref HealthEvent);

            ctx.Event.User.SendSystemMessage($"Player \"{PlayerName}\" Definida para <color=#ffff00ff>{Value}%</color>");
        }
    }
}
