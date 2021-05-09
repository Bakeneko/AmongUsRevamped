using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using InnerNet;
using UnhollowerBaseLib;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public partial class Game
    {
#if STEAM
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Steamworks.SteamAPI), nameof(Steamworks.SteamAPI.RestartAppIfNecessary))]
        private static bool SteamAPIRestartAppIfNecessaryPatch(ref bool __result)
        {
            // Ensure steam app id file existence
            const string file = "steam_appid.txt";
            const string content = "945360";
            try
            {
                if(!File.Exists(file) || !content.Equals(File.ReadAllText(file)))
                    File.WriteAllText(file, content);
            }
            catch {}
            return __result = false;
        }
#endif

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
        private static void ChatControllerAwakePatch()
        {
            if (EOSManager.Instance.IsMinor())
            {
                SaveManager.chatModeType = (int)QuickChatModes.QuickChatOnly;
                SaveManager.isGuest = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
        private static bool GameDataGetAvailableIdPatch(GameData __instance, out sbyte __result)
        {
            for (sbyte i = 0; i < sbyte.MaxValue; i++)
            {
                if (!__instance.AllPlayers.ToArray().Any(p => p.PlayerId == i))
                {
                    __result = i;
                    return false;
                }
            }

            __result = -1;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        private static bool LobbyBehaviourStartPatch(LobbyBehaviour __instance)
        {
            OnLobbyStart(__instance);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        private static bool GameStartManagerBeginGamePatch()
        {
            return OnBegin();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        private static void ShipStatusStartPatch()
        {
            OnGameStart();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
        public static bool AmongUsClientExitGamePatch()
        {
            OnExit();
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        private static bool ShipStatusCalculateLightRadiusPatch(ShipStatus __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            return CalculateLightRadius(__instance, player, ref __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        private static void HudManagerStartPatch(HudManager __instance)
        {
            OnHudStart(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        private static void HudManagerUpdatePatch(HudManager __instance)
        {
            OnHudUpdate(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        private static bool PlayerControlStartPatch(PlayerControl __instance)
        {
            OnPlayerStart(__instance);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        private static void PlayerControlFixedUpdatePatch(PlayerControl __instance)
        {
            OnPlayerFixedUpdate(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        private static void PlayerPhysicsFixedUpdatePatch(PlayerPhysics __instance)
        {
            OnPlayerPhysicsFixedUpdate(__instance.myPlayer, __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
        private static void CustomNetworkTransformFixedUpdatePatch(CustomNetworkTransform __instance)
        {
            OnPlayerNetworkTransformFixedUpdate(__instance.GetComponent<PlayerControl>(), __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static void PingTrackerUpdatePatch(PingTracker __instance)
        {
            OnPingTrackerUpdate(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        private static void PlayerControlRpcSetInfectedPatch([HarmonyArgument(0)] Il2CppReferenceArray<GameData.PlayerInfo> impostors)
        {
            OnDistributeRoles();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        private static void IntroCutsceneBeginPatch(IntroCutscene __instance, [HarmonyArgument(0)] ref Il2CppSystem.Collections.Generic.List<PlayerControl> team)
        {
            OnIntroStart(__instance, ref team);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(IntroCutscene.Nested_0), nameof(IntroCutscene.Nested_0.MoveNext))]
        private static void IntroCutsceneMoveNextPatch(IntroCutscene.Nested_0 __instance)
        {
            OnIntroUpdate(__instance);
        }

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        private static void ExileControllerBeginPatch([HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
        {
            OnExileBegin(exiled, tie);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        private static bool ExileControllerWrapUpPatch(ExileController __instance)
        {
            OnExileEnd(__instance);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__78), nameof(PlayerControl._CoSetTasks_d__78.MoveNext))]
        private static void PlayerControlSetTasksMoveNextPatch(PlayerControl._CoSetTasks_d__78 __instance)
        {
            if (__instance?.__this == null) return;
            OnPlayerTasksCreated(__instance?.__this);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static bool GameDataRecomputeTaskCountsPatch()
        {
            RecomputeTaskCount();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
        private static void EmergencyMinigameUpdatePatch(EmergencyMinigame __instance)
        {
            OnEmergencyButtonUpdate(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
        private static bool ConsoleCanUsePatch(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo player,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            return OnPlayerCanUseConsole(player, __instance, ref __result, out canUse, out couldUse);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        private static bool VentCanUsePatch(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo player,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            OnPlayerCanUseVent(player, __instance, ref __result, out canUse, out couldUse);
            return false;
        }

        /// <summary>
        /// Allow to kill anyone
        /// </summary>
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        private static class PlayerControlMurderPlayerPatch
        {
            private static bool wasDead = false;
            private static bool wasImpostor = false;

            private static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                wasDead = __instance.Data.IsDead;
                wasImpostor = __instance.Data.IsImpostor;
                __instance.Data.IsDead = false;
                __instance.Data.IsImpostor = true;
            }

            private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                __instance.Data.IsDead = wasDead || __instance.PlayerId == target.PlayerId; // Dead or killed himself
                __instance.Data.IsImpostor = wasImpostor;
                OnPlayerMurderedPlayer(__instance, target);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
        private static bool PlayerControlRpcMurderPlayerPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            MurderRpc.Instance.Send(new Tuple<byte, byte>(__instance.PlayerId, target.PlayerId));
            __instance.MurderPlayer(target);
            return false;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
        private static class PlayerControlCmdReportDeadBodyPatch
        {
            private static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]GameData.PlayerInfo victim)
            {
                if (victim == null)
                {
                    return OnPlayerCallMeeting(__instance);
                }
                else
                {
                    return OnPlayerReportBody(__instance, victim);
                }
            }

            private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo victim)
            {
                if (victim == null)
                {
                    OnPlayerCalledMeeting(__instance);
                }
                else
                {
                    OnPlayerReportedBody(__instance, victim);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
        private static bool PlayerControlDiePatch(PlayerControl __instance)
        {
            return OnPlayerDie(__instance);
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        private static class PlayerControlExiledPatch
        {
            private static bool Prefix(PlayerControl __instance)
            {
                return OnPlayerExile(__instance);
            }

            private static void Postfix(PlayerControl __instance)
            {
                OnPlayerExiled(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        private static void TranslationControllerGetStringPatch(ref string __result, [HarmonyArgument(0)] StringNames id, [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts)
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                __result = GetTextForExile(ExileController.Instance.exiled, id) ?? __result;
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
        private static void PlayerControlCompleteTaskPatch(PlayerControl __instance, [HarmonyArgument(0)] uint taskIndex)
        {
            GameData.TaskInfo task = null;
            try
            {
                task = __instance.Data.Tasks[(int)taskIndex];
            }
            catch
            {
            }
            OnPlayerCompletedTask(__instance, task);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        private static bool ShipStatusCheckEndCriteriaPatch(ShipStatus __instance)
        {
            return CheckEnd(__instance);
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        private class AmongUsClientOnGameEndPatch
        {
            private static void Prefix([HarmonyArgument(0)] ref GameOverReason reason, [HarmonyArgument(1)] bool _)
            {
                if (GameOver == null)
                {
                    GameOver = new GameOverData((CustomGameOverReason)reason);
                }
                if ((int)reason > (int)GameOverReason.HumansDisconnect) reason = GameOverReason.ImpostorByKill;
            }

            private static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] GameOverReason reason, [HarmonyArgument(1)] bool showAd)
            {
                OnEnd(__instance, GameOver);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        private static void EndGameManagerSetEverythingUpPatch(EndGameManager __instance)
        {
            OnGameOver(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        private static void ShipStatusIsGameOverDueToDeathPatch(ref bool __result)
        {
            __result = IsGameOverDueToDeath();
        }
    }
}
