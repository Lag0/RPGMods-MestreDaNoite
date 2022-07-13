using ProjectM;
using RPGMods.Utils;
using Wetstone.API;
using System.Text.RegularExpressions;

namespace RPGMods.Commands
{
    [Command("save", Usage = "save [<name>]", Description = "Força o servidor a salvar o jogo, bem como gravar RPGMods DB em um arquivo json.")]
    public static class Save
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            string name = "Manual Save";
            if (args.Length >= 1)
            {
                name = string.Join(' ', ctx.Args);
                if (name.Length > 50)
                {
                    Output.CustomErrorMessage(ctx, "Nome muito grande!");
                    return;
                }
                if (Regex.IsMatch(name, @"[^a-zA-Z0-9\x20]"))
                {
                    Output.CustomErrorMessage(ctx, "O nome só pode conter letras e espaço!");
                    return;
                }
            }
            
            ctx.Event.User.SendSystemMessage($"Salvando....");
            //AutoSaveSystem.SaveDatabase();
            VWorld.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, name);
            ctx.Event.User.SendSystemMessage($"Save completo");
        }
    }
}
