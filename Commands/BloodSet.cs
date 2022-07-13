using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("blood", Usage = "blood <Type> [<Quality>] [<Value>]", Description = "Define seu tipo de sangue, qualidade e valor específicos")]
    public static class BloodSet
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                try
                {
                    PrefabGUID type = new PrefabGUID();
                    float quality = 100;
                    int value = 100;

                    if (ctx.Args.Length >= 1) type = Helper.GetSourceTypeFromName(ctx.Args[0]);
                    if (ctx.Args.Length >= 2)
                    {
                        quality = float.Parse(ctx.Args[1]);
                        if (float.Parse(ctx.Args[1]) < 0) quality = 0;
                        if (float.Parse(ctx.Args[1]) > 100) quality = 100;
                    }
                    if (ctx.Args.Length >= 3) value = int.Parse(ctx.Args[2]);

                    var BloodEvent = new ChangeBloodDebugEvent()
                    {
                        Amount = value,
                        Quality = quality,
                        Source = type
                    };
                    VWorld.Server.GetExistingSystem<DebugEventsSystem>().ChangeBloodEvent(ctx.Event.User.Index, ref BloodEvent);
                    ctx.Event.User.SendSystemMessage($"Sangue alterado para <color=#ffff00ff>{ctx.Args[0]}</color> com <color=#ffff00ff>{quality}</color>% de qualidade");
                }
                catch
                {
                    Utils.Output.InvalidArguments(ctx);
                }

            }
            else
            {
                Utils.Output.MissingArguments(ctx);
            }
        }
    }
}
