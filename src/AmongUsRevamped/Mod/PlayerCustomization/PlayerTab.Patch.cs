using AmongUsRevamped.Options;
using HarmonyLib;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [HarmonyPatch]
    class PlayerTabPatch
    {
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        [HarmonyPostfix]
        public static void PlayerTabOnEnablePatch(PlayerTab __instance)
        {
            foreach (ColorChip chip in __instance.ColorChips)
            {
                chip.transform.localScale *= 0.7f;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.SelectColor))]
        [HarmonyPostfix]
        public static void PlayerTabSelectColorPatch([HarmonyArgument(0)] int colorIndex)
        {
            CustomOption.BodyColor.Value = colorIndex;
            SaveManager.BodyColor = (byte)(colorIndex < 12 ? colorIndex : 0);
        }
    }
}
