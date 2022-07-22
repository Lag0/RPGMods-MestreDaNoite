using HarmonyLib;
using ProjectM;
using MDNMods.Utils;

namespace MDNMods.Hooks
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }
}
