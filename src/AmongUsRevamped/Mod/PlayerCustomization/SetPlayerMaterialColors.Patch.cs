using HarmonyLib;
using Reactor.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPlayerMaterialColors), typeof(int), typeof(Renderer))]
    public class SetPlayerMaterialPatch
    {
        public static bool Prefix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer renderer)
        {
            var pcb = renderer.gameObject.GetComponent<PlayerColorBehaviour>();

            if (!PlayerColorUtils.IsRainbow(colorId))
            {
                pcb?.Destroy();
                return true;
            }

            if (pcb == null)
            {
                pcb = renderer.gameObject.AddComponent<PlayerColorBehaviour>();
            }
            pcb.SetRenderer(renderer, colorId);

            return false;
        }
    }
}
