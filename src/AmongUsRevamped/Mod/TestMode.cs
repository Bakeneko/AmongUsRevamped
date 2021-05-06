using System;
using System.Linq;
using AmongUsRevamped.Extensions;
using HarmonyLib;
using InnerNet;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class TestMode
    {
        private static GameObject GameObject;
        private static TestModeComponent Component;

        private static int DefaultMinPlayers;
        private static float DefaultCountDownTimer;

        private static int[] DefaultMinPlayersValues;
        private static int[] DefaultRecommendedImpostorsValues;
        private static int[] MDefaultaxImpostorsValues;

        public static bool DisableGameEnd => Options.Values.TestMode && (Component?.DisableGameEnd ?? false);

        public static void Load()
        {
            Component?.Destroy();
            Component = null;
            GameObject?.Destroy();
            GameObject = null;

            GameObject ??= new GameObject(nameof(TestMode)).DontDestroy();
            Component ??= GameObject.AddComponent<TestModeComponent>();

            DefaultMinPlayersValues = GameOptionsData.MinPlayers;
            MDefaultaxImpostorsValues = GameOptionsData.MaxImpostors;
            DefaultRecommendedImpostorsValues = GameOptionsData.RecommendedImpostors;

            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat(1, 4).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(1, 4).ToArray();

            var gameStartManager = GameStartManager.Instance;
            if (gameStartManager != null)
            {
                DefaultMinPlayers = gameStartManager.MinPlayers;
                DefaultCountDownTimer = gameStartManager.countDownTimer;
                gameStartManager.MinPlayers = 1;
            }
        }

        public static void Unload()
        {
            var gameStartManager = GameStartManager.Instance;
            if (gameStartManager != null)
            {
                gameStartManager.MinPlayers = DefaultMinPlayers;
                gameStartManager.countDownTimer = DefaultCountDownTimer;
            }

            DefaultMinPlayersValues = GameOptionsData.MinPlayers;
            MDefaultaxImpostorsValues = GameOptionsData.MaxImpostors;
            DefaultRecommendedImpostorsValues = GameOptionsData.RecommendedImpostors;

            GameOptionsData.MaxImpostors = MDefaultaxImpostorsValues;
            GameOptionsData.RecommendedImpostors = DefaultRecommendedImpostorsValues;
            GameOptionsData.MinPlayers = DefaultMinPlayersValues;

            Component?.Destroy();
            Component = null;
            GameObject?.Destroy();
            GameObject = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        private static bool GameStartManagerUpdatePatch(GameStartManager __instance)
        {
            if (!Options.Values.TestMode) return true;
            __instance.MinPlayers = 1;
            __instance.countDownTimer = 0;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        private static void GameOptionsMenuStartPatch(GameOptionsMenu __instance)
        {
            __instance.Children
                .Single(o => o.Title == StringNames.GameNumImpostors)
                .Cast<NumberOption>()
                .ValidRange = new FloatRange(0, byte.MaxValue);
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        private static bool ShipStatusCheckEndCriteriaPatch()
        {
            return !DisableGameEnd;
        }   
    }

    [RegisterInIl2Cpp]
    public class TestModeComponent : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public DragWindow TestWindow { get; }

        [HideFromIl2Cpp]
        public bool DisableGameEnd { get; set; }

        public TestModeComponent(IntPtr ptr) : base(ptr)
        {
            TestWindow = new DragWindow(new Rect(0, Screen.height * 0.5f, 100, 100), "Test Mode", () =>
            {
                GUILayout.Label($"Name: {PlayerControl.LocalPlayer?.Data?.PlayerName ?? SaveManager.PlayerName}", new Il2CppReferenceArray<GUILayoutOption>(0));

                DisableGameEnd = GUILayout.Toggle(DisableGameEnd, "Disable game end", new Il2CppReferenceArray<GUILayoutOption>(0));

                if (AmongUsClient.Instance.AmHost && ShipStatus.Instance && GUILayout.Button("Call a meeting", new Il2CppReferenceArray<GUILayoutOption>(0)))
                {
                    CallForMeeting();
                }

                if ((LobbyBehaviour.Instance || ShipStatus.Instance) && GUILayout.Button("Spawn a dummy", new Il2CppReferenceArray<GUILayoutOption>(0)))
                {
                    SpawnDummy();
                }

                if (AmongUsClient.Instance.AmHost && ShipStatus.Instance && GUILayout.Button("End game", new Il2CppReferenceArray<GUILayoutOption>(0)))
                {
                    EndGame();
                }

                if (PlayerControl.LocalPlayer)
                {
                    var position = PlayerControl.LocalPlayer.transform.position;
                    GUILayout.Label($"x: {position.x}", new Il2CppReferenceArray<GUILayoutOption>(0));
                    GUILayout.Label($"y: {position.y}", new Il2CppReferenceArray<GUILayoutOption>(0));
                }
            })
            {
                Enabled = Options.Values.TestMode
            };
        }

        private void CallForMeeting()
        {
            MeetingRoomManager.Instance.reporter = PlayerControl.LocalPlayer;
            MeetingRoomManager.Instance.target = null;
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.RpcStartMeeting(null);
        }

        private void SpawnDummy()
        {
            var playerControl = Instantiate(AmongUsClient.Instance.PlayerPrefab);
            var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
            GameData.Instance.AddPlayer(playerControl);
            AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
            playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            playerControl.NetTransform.enabled = false;
            playerControl.SetName($"{TranslationController.Instance.GetString(StringNames.Dummy, Array.Empty<Il2CppSystem.Object>())} {i}");
            playerControl.SetColor((byte)(i % Palette.PlayerColors.Length));
            playerControl.SetHat(i % (uint)HatManager.Instance.AllHats.Count, playerControl.Data.ColorId);
            playerControl.SetPet(i % (uint)HatManager.Instance.AllPets.Count);
            playerControl.SetSkin(i % (uint)HatManager.Instance.AllSkins.Count);
            GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
        }

        private void EndGame()
        {
            ShipStatus.Instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
        }

        protected void Update()
        {
            if (!Options.Values.TestMode)
            {
                TestWindow.Enabled = false;
            }
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                TestWindow.Enabled = !TestWindow.Enabled;
            }
        }

        protected void OnGUI()
        {
            TestWindow.OnGUI();
        }
    }
}
