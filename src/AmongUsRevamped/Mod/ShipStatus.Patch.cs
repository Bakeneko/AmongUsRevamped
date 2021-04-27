using HarmonyLib;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class ShipStatusPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool ShipStatusCalculateLightRadiusPatch(ShipStatus __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            SwitchSystem switchSystem = systemType?.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float light = switchSystem.Value / 255f;

            if (player == null || player.IsDead) // Ghost
            {
                __result = __instance.MaxLightRadius;
            }
            else if (player.IsImpostor) // Impostor
            {
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            }
            else // Crew
            { 
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, light) * PlayerControl.GameOptions.CrewLightMod;
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public static void ShipStatusIsGameOverDueToDeathPatch(ref bool __result)
        {
            __result = false;
        }
    }
}
