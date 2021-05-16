using System;
using System.Linq;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Mod.Roles;
using AmongUsRevamped.Mod.Modifiers;
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

        private static int DefaultMinPlayers = -1;
        private static float DefaultCountDownTimer = -1;

        private static int[] DefaultMinPlayersValues;
        private static int[] DefaultRecommendedImpostorsValues;
        private static int[] DefaultMaxImpostorsValues;

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
            DefaultMaxImpostorsValues = GameOptionsData.MaxImpostors;
            DefaultRecommendedImpostorsValues = GameOptionsData.RecommendedImpostors;

            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat(3, 16).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(1, 15).ToArray();

            var gameStartManager = GameStartManager.Instance;
            if (gameStartManager != null)
            {
                DefaultMinPlayers = gameStartManager.MinPlayers;
                DefaultCountDownTimer = gameStartManager.countDownTimer;
                gameStartManager.MinPlayers = 1;
                gameStartManager.countDownTimer = 0;
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

            GameOptionsData.MinPlayers = DefaultMinPlayersValues;

            Component?.Destroy();
            Component = null;
            GameObject?.Destroy();
            GameObject = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        private static void GameStartManagerUpdatePatch(GameStartManager __instance)
        {
            if (Options.Values.TestMode && DefaultMinPlayers != -1)
            {
                __instance.MinPlayers = 1;
                __instance.countDownTimer = 0;
            }
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
            Vector2 scrollPosition = Vector2.zero;

            TestWindow = new DragWindow(new Rect(0, Screen.height * 0.5f, 150, 150), "Test Mode", () =>
            {
                GUILayout.Label($"Name: {PlayerControl.LocalPlayer?.Data?.PlayerName ?? SaveManager.PlayerName}", new Il2CppReferenceArray<GUILayoutOption>(0));

                DisableGameEnd = GUILayout.Toggle(DisableGameEnd, "Disable game end", new Il2CppReferenceArray<GUILayoutOption>(0));

                if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                {
                    
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(200) });

                    GUILayout.Label($"Roles", new Il2CppReferenceArray<GUILayoutOption>(0));
                    if (GUILayout.Button("Crewmate", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Crewmate);
                    if (GUILayout.Button("Engineer", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Engineer);
                    if (GUILayout.Button("Sheriff", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Sheriff);
                    if (GUILayout.Button("Snitch", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Snitch);
                    if (GUILayout.Button("Spy", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Spy);
                    if (GUILayout.Button("TimeLord", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.TimeLord);
                    if (GUILayout.Button("Camouflager", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Camouflager);
                    if (GUILayout.Button("Cleaner", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Cleaner);
                    if (GUILayout.Button("Impostor", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Impostor);
                    if (GUILayout.Button("Morphling", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Morphling);
                    if (GUILayout.Button("Swooper", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Swooper);
                    if (GUILayout.Button("Jester", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeRole(RoleType.Jester);

                    GUILayout.Label($"Modifiers", new Il2CppReferenceArray<GUILayoutOption>(0));
                    if (GUILayout.Button("Drunk", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeModifier(ModifierType.Drunk);
                    if (GUILayout.Button("Flash", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeModifier(ModifierType.Flash);
                    if (GUILayout.Button("Giant", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeModifier(ModifierType.Giant);
                    if (GUILayout.Button("Torch", new Il2CppReferenceArray<GUILayoutOption>(0))) ChangeModifier(ModifierType.Torch);

                    GUILayout.EndScrollView();
                }

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

        private void ChangeRole(RoleType role)
        {
            Game.AssignPlayerRole(PlayerControl.LocalPlayer, role);
        }

        private void ChangeModifier(ModifierType modifier)
        {
            Game.AssignPlayerModifier(PlayerControl.LocalPlayer, modifier);
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
            var winners = Role.AllRoles.Where(r => r.Faction == Faction.Crewmates).Select(r => r.Player).ToList();
            Game.EndGame(Game.CustomGameOverReason.ImpostorDisconnect, winners);
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
