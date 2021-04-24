using System;
using System.Linq;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using HarmonyLib;
using UnhollowerBaseLib;

namespace AmongUsRevamped.Events
{
    [HarmonyPatch]
    public static partial class GameEvents
    {
        public static event EventHandler<VoteCastedEventArgs> VoteCasted;
        public static event EventHandler<VotingCompletedEventArgs> VotingCompleted;
        public static event EventHandler<VentEventArgs> VentEntered;
        public static event EventHandler<VentEventArgs> VentExited;
        public static event EventHandler<PlayerMurderedEventArgs> PlayerMurdered;
        public static event EventHandler<BodyReportedEventArgs> BodyReported;
        public static event EventHandler<EventArgs> MeetingCalled;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
        [HarmonyPrefix]
        private static void CastVote(
            MeetingHud __instance,
            [HarmonyArgument(0)] byte votingPlayerId,
            [HarmonyArgument(1)] sbyte votedPlayerId)
        {
            var player = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(pi => pi.PlayerId == votingPlayerId);
            var voted = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(pi => pi.PlayerId == votedPlayerId);
            VoteCasted?.SafeInvoke(__instance, new VoteCastedEventArgs(player, voted), nameof(VoteCasted));
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        [HarmonyPostfix]
        private static void VotingComplete(
            MeetingHud __instance,
            [HarmonyArgument(0)] Il2CppStructArray<byte> states,
            [HarmonyArgument(1)] GameData.PlayerInfo ejectedPlayer,
            [HarmonyArgument(2)] bool EMBDDLIPBME)
        {
            VotingCompleted?.SafeInvoke(__instance, new VotingCompletedEventArgs(ejectedPlayer), nameof(VotingCompleted));
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent), typeof(PlayerControl))]
        [HarmonyPrefix]
        private static void EnterVent(Vent __instance, [HarmonyArgument(0)] PlayerControl player)
        {
            var system = ShipUtils.GetSystem(player.GetTruePosition());
            VentEntered?.SafeInvoke(__instance, new VentEventArgs(system, player), nameof(VentEntered));
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent), typeof(PlayerControl))]
        [HarmonyPrefix]
        private static void ExitVent(Vent __instance, [HarmonyArgument(0)] PlayerControl player)
        {
            var system = ShipUtils.GetSystem(player.GetTruePosition());
            VentExited?.SafeInvoke(__instance, new VentEventArgs(system, player), nameof(VentExited));
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        private static void MurderPlayer(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl victim)
        {
            var system = ShipUtils.GetSystem(victim.GetTruePosition());
            PlayerMurdered?.SafeInvoke(__instance, new PlayerMurderedEventArgs(__instance, victim, system), nameof(PlayerMurdered));
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
        [HarmonyPrefix]
        private static void ReportDeadBodyOrButton(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo victim)
        {
            if (victim != null)
            {
                var victimControls = PlayerControl.AllPlayerControls.ToArray().First(pc => pc.PlayerId == victim.PlayerId);
                var system = ShipUtils.GetSystem(victimControls?.GetTruePosition() ?? __instance.GetTruePosition());
                BodyReported?.SafeInvoke(__instance, new BodyReportedEventArgs(__instance, victim, system), nameof(BodyReported));
            }
            else
            {
                MeetingCalled?.SafeInvoke(__instance, EventArgs.Empty, nameof(MeetingCalled));
            }
        }
    }
}
