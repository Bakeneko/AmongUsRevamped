using HarmonyLib;

namespace AmongUsRevamped.Mod
{
    /// <summary>
    /// Deactivate bans
    /// </summary>
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}
