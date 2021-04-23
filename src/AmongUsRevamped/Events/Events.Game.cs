using System;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using HarmonyLib;

namespace AmongUsRevamped.Events
{
    [HarmonyPatch]
    public static partial class GameEvents
    {
        public static event EventHandler<VentEventArgs> VentEntered;
        public static event EventHandler<VentEventArgs> VentExited;

        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent), typeof(PlayerControl))]
        [HarmonyPrefix]
        public static void EnterVent(Vent __instance, [HarmonyArgument(0)] PlayerControl player)
        {
            var system = ShipUtils.GetSystem(player.GetTruePosition());
            VentEntered?.SafeInvoke(null, new VentEventArgs(__instance.Id, __instance.name, system, player), nameof(VentEntered));
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent), typeof(PlayerControl))]
        [HarmonyPrefix]
        public static void ExitVent(Vent __instance, [HarmonyArgument(0)] PlayerControl player)
        {
            var system = ShipUtils.GetSystem(player.GetTruePosition());
            VentExited?.SafeInvoke(null, new VentEventArgs(__instance.Id, __instance.name, system, player), nameof(VentExited));
        }
    }
}
