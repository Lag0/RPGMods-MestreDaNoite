using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("teleport, tp", "teleport <Nome>", "Teleporta você para outro jogador online dentro do seu clã.")]
    public static class Teleport
    {
        public static void Initialize(Context ctx)
        {
            var eventUser = ctx.Event.User;
            var UserCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;

            if (Helper.IsPlayerInCombat(UserCharacter))
            {
                Utils.Output.CustomErrorMessage(ctx, "Não foi possível usar o comando! Você está em combate!");
                return;
            }
            if (ctx.Args.Length < 1)
            {
                Utils.Output.InvalidArguments(ctx);
                return;
            }

            Team user_TeamComponent = entityManager.GetComponentData<Team>(UserCharacter);

            string TargetName = string.Join(' ', ctx.Args);
            LocalToWorld target_WorldComponent;
            Team target_TeamComponent;

            if (Helper.FindPlayer(TargetName, true, out Entity TargetChar, out Entity TargetUserEntity))
            {
                target_TeamComponent = entityManager.GetComponentData<Team>(TargetUserEntity);
                target_WorldComponent = entityManager.GetComponentData<LocalToWorld>(TargetChar);
            }
            else
            {
                Utils.Output.CustomErrorMessage(ctx, "Jogador alvo não encontrado.");
                return;
            }

            var serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            if (!serverGameManager._TeamChecker.IsAllies(user_TeamComponent, target_TeamComponent))
            {
                Utils.Output.CustomErrorMessage(ctx, "Incapaz de se teletransportar para um jogador de outro clã!");
                return;
            }

            if (Helper.IsPlayerInCombat(TargetChar))
            {
                Utils.Output.CustomErrorMessage(ctx, $"Incapaz de se teletransportar! Jogador \"{TargetName}\" está em combate!");
                return;
            }

            Helper.TeleportTo(ctx, new(target_WorldComponent.Position.x, target_WorldComponent.Position.z));
        }
    }
}
