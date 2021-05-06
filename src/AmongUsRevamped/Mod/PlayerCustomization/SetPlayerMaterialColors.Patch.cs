using AmongUsRevamped.Extensions;
using HarmonyLib;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [HarmonyPatch]
    public static class SetPlayerMaterialColorsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors), typeof(int), typeof(Renderer))]
        public static bool SetPlayerMaterialColorsIntPatch([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
        {
            var pcb = renderer.gameObject.GetComponent<PlayerColorBehaviour>();

            if (!PlayerColorUtils.IsRainbow(colorId))
            {
                pcb?.Destroy();
                return true;
            }

            pcb ??= renderer.gameObject.AddComponent<PlayerColorRainbowBehaviour>();
            pcb.SetRenderer(renderer);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors), typeof(Color), typeof(Renderer))]
        public static bool SetPlayerMaterialColorsColorPatch([HarmonyArgument(0)] Color color, [HarmonyArgument(1)] Renderer renderer)
        {
            var pcb = renderer.gameObject.GetComponent<PlayerColorBehaviour>();

            if (!PlayerColorUtils.IsRainbow(color))
            {
                pcb?.Destroy();
                return true;
            }

            pcb ??= renderer.gameObject.AddComponent<PlayerColorRainbowBehaviour>();
            pcb.SetRenderer(renderer);

            return false;
        }
    }
}
