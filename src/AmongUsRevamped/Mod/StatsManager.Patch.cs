using HarmonyLib;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class StatsManagerPatch
    {
        /// <summary>
        /// Deactivate bans
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.BanPoints), MethodType.Getter)]
        public static void BanPointsGetterPatch(ref float __result)
        {
            __result = 0;
        }

        /// <summary>
        /// Deactivate bans
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.BanPoints), MethodType.Setter)]
        public static void BanPointsSetterPatch([HarmonyArgument(0)]ref float points)
        {
            points = 0;
        }

        /// <summary>
        /// Deactivate bans
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static void AmBannedPatch(ref bool __result)
        {
            __result = false;
        }
    }
}
