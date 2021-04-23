using AmongUsRevamped.Colors;
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

            if (!PlayerColorUtils.IsRainbow(Palette.ShortColorNames[colorId]))
            {
                pcb?.Destroy();
                return true;
            }

            if (pcb == null)
            {
                pcb = renderer.gameObject.AddComponent<PlayerColorBehaviour>();
            }
            pcb.SetRenderer(renderer, ColorPalette.ShortColorName.Rainbow);

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

            if (pcb == null)
            {
                pcb = renderer.gameObject.AddComponent<PlayerColorBehaviour>();
            }
            pcb.SetRenderer(renderer, ColorPalette.ShortColorName.Rainbow);

            return false;
        }
    }
}
