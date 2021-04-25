using HarmonyLib;
using Reactor.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors), typeof(int), typeof(Renderer))]
    public class SetPlayerMaterialColorsIntPatch
    {
        public static bool Prefix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
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
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors), typeof(Color), typeof(Renderer))]
    public class SetPlayerMaterialColorsColorPatch
    {
        public static bool Prefix([HarmonyArgument(0)] Color color, [HarmonyArgument(1)] Renderer renderer)
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
