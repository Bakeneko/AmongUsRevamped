using System;
using AmongUsRevamped.Extensions;
using HarmonyLib;

namespace AmongUsRevamped.Events
{
    [HarmonyPatch]
    public static partial class HudEvents
    {
        public static event EventHandler<EventArgs> HudCreated;
        public static event EventHandler<EventArgs> OnHudUpdate;
        public static event EventHandler<EventArgs> HudUpdated;
        public static event EventHandler<HudStateChangedEventArgs> HudStateChanged;
        public static event EventHandler<ResolutionChangedEventArgs> ResolutionChanged;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        private static void HudManagerStart()
        {
            HudCreated?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(HudCreated));
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        private static class HudManagerUpdate
        {
            private static bool Prefix()
            {
                OnHudUpdate?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(OnHudUpdate));
                return true;
            }

            private static void Postfix()
            {
                HudUpdated?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(HudUpdated));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        private static void HudManagerSetHudActive([HarmonyArgument(0)] bool isActive)
        {
            HudStateChanged?.SafeInvoke(HudManager.Instance, new HudStateChangedEventArgs(isActive), nameof(HudStateChanged));
        }

        internal static void RaiseResolutionChanged(int oldPixelWidth, int oldPixelHeight, float oldWidth, float oldHeight)
        {
            ResolutionChanged?.SafeInvoke(HudManager.Instance, new ResolutionChangedEventArgs(oldPixelWidth, oldPixelHeight, oldWidth, oldHeight), nameof(ResolutionChanged));
        }
    }
}
