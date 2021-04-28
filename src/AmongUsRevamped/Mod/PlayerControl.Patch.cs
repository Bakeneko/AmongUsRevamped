using HarmonyLib;
using InnerNet;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class PlayerControlPatch
    {
        /// <summary>
        /// Fix player ability to move during events
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
        public static bool PlayerControlCanMovePatch(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                !IntroCutscene.Instance &&
                !Minigame.Instance &&
                !MeetingHud.Instance &&
                !CustomPlayerMenu.Instance &&
                !ExileController.Instance &&
                (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpen && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)) &&
                (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped);
            
            return false;
        }
    }
}
