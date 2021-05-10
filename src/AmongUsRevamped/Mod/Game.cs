using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Mod.Modifiers;
using AmongUsRevamped.Mod.Roles;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using HarmonyLib;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public partial class Game
    {
        private static float BasePlayerSpeed = AmongUsClient.Instance?.PlayerPrefab?.MyPhysics?.Speed ?? 2.5f;

        private static GameOverData GameOver = null;

        private static void OnExit()
        {
            Reset();
        }

        private static void OnLobbyStart(LobbyBehaviour lobby)
        {
            Reset();
        }

        private static void Reset()
        {
            BasePlayerSpeed = AmongUsClient.Instance?.PlayerPrefab?.MyPhysics?.Speed ?? 2.5f;
            GameOver = null;
            Role.AllRoles.ForEach(r => r.Dispose());
            Modifier.AllModifiers.ForEach(m => m.Dispose());
        }

        private static bool OnBegin()
        {
            return true;
        }

        private static bool CalculateLightRadius(ShipStatus shipStatus, Player player, ref float radius)
        {
            ISystemType systemType = shipStatus.Systems.ContainsKey(SystemTypes.Electrical) ? shipStatus.Systems[SystemTypes.Electrical] : null;
            SwitchSystem switchSystem = systemType?.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float visionRange = player.VisionRange;
            if (visionRange <= 0) _ = PlayerControl.GameOptions.CrewLightMod;

            float light = switchSystem.Value / 255f;

            if (player == null || player.IsDead) // Ghost
            {
                visionRange = 1f;
                light = 1f;
            }
            else if (player.HasNightVision) // Role or modifier with night vision
            {
                light = 1f;
            }

            radius = Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, light) * visionRange;

            return false;
        }

        private static void OnGameStart()
        {

        }

        private static void OnPlayerStart(Player player)
        {

        }

        private static void OnPlayerFixedUpdate(Player player)
        {
            player?.OnFixedUpdate();
        }

        /// <summary>
        /// Mess with player physic
        /// </summary>
        private static void OnPlayerPhysicsFixedUpdate(Player player, PlayerPhysics physics)
        {
            if (physics.AmOwner)
            {
                physics.body.velocity *= player?.MoveSpeed ?? 1f;
            }
        }

        /// <summary>
        /// Mess with player movement
        /// </summary>
        private static void OnPlayerNetworkTransformFixedUpdate(Player player, CustomNetworkTransform networkTransform)
        {
            if (!networkTransform.AmOwner)
            {
                networkTransform.body.velocity *= player?.MoveSpeed ?? 1f;
            }
        }

        private static void OnPingTrackerUpdate(PingTracker pingTracker)
        {
            pingTracker.text.fontSize = 2.5f;
            pingTracker.text.transform.localPosition = new HudPosition(2.5f, 0.35f, HudAlignment.TopRight);

            pingTracker.text.text = ColorPalette.Color.Revamped.ToColorTag($"{AmongUsRevamped.Name} v{AmongUsRevamped.VersionString}");
            pingTracker.text.text += $"\nPing: {AmongUsClient.Instance.Ping} ms";
        }

        private static void OnHudStart(HudManager hudManager)
        {

        }

        private static void OnHudUpdate(HudManager hudManager)
        {
            foreach(Player p in Player.AllPlayers) { p.HudUpdate(hudManager); };
            Player.CurrentPlayer?.CurrentPlayerHudUpdate(hudManager);
        }

        private static void OnDistributeRoles()
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists) return;

            List<Player> crewmates = new(), impostors = new(), players = Player.AllPlayers.ToList();
            foreach (Player p in players)
            {
                if (p.IsImpostor) impostors.Add(p);
                else crewmates.Add(p);
            }

            int maxCrewmateRoles = Mathf.Min(crewmates.Count, Options.Values.MaxCrewmateRoles);
            var generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.SheriffSpawnRate > 0) generator.AddNumber((byte)RoleType.Sheriff, Options.Values.SheriffSpawnRate);
            if (Options.Values.TimeLordSpawnRate > 0) generator.AddNumber((byte)RoleType.TimeLord, Options.Values.TimeLordSpawnRate);
            if (Options.Values.JesterSpawnRate > 0) generator.AddNumber((byte)RoleType.Jester, Options.Values.JesterSpawnRate);

            for (int i = 0; i < maxCrewmateRoles; i++)
            {
                if (generator.GetNumberCount() == 0) break; // No more special roles, assign base crewmate role

                var role = generator.GetDistributedRandomNumber();
                generator.RemoveNumber(role);
                AssignRoleRandomly((RoleType)role, crewmates);
            }
            crewmates.ForEach(p => AssignPlayerRole(p, RoleType.Crewmate));

            int maxImpostorRoles = Mathf.Min(impostors.Count, Options.Values.MaxImpostorRoles);
            generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.CleanerSpawnRate > 0) generator.AddNumber((byte)RoleType.Cleaner, Options.Values.CleanerSpawnRate);
            if (Options.Values.SwooperSpawnRate > 0) generator.AddNumber((byte)RoleType.Swooper, Options.Values.SwooperSpawnRate);

            for (int i = 0; i < maxImpostorRoles; i++)
            {
                if (generator.GetNumberCount() == 0) break; // No more special roles, assign base impostor role

                var role = generator.GetDistributedRandomNumber();
                generator.RemoveNumber(role);
                AssignRoleRandomly((RoleType)role, impostors);
            }
            impostors.ForEach(p => AssignPlayerRole(p, RoleType.Impostor));

            int maxModifiers = Mathf.Min(players.Count, Options.Values.MaxModifiers);
            generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.DrunkSpawnRate > 0) generator.AddNumber((byte)ModifierType.Drunk, Options.Values.DrunkSpawnRate);
            if (Options.Values.FlashSpawnRate > 0) generator.AddNumber((byte)ModifierType.Flash, Options.Values.FlashSpawnRate);
            if (Options.Values.TorchSpawnRate > 0) generator.AddNumber((byte)ModifierType.Torch, Options.Values.TorchSpawnRate);

            for (int i = 0; i < maxModifiers; i++)
            {
                if (generator.GetNumberCount() > 0)
                {
                    var role = generator.GetDistributedRandomNumber();
                    generator.RemoveNumber(role);
                    AssignModifierRandomly((ModifierType)role, players);
                }
            }
        }

        public static void AssignRoleRandomly(RoleType type, List<Player> players)
        {
            int index = AmongUsRevamped.Rand.Next(0, players.Count);
            Player player = players[index];
            players.RemoveAt(index);
            AssignPlayerRole(player, type);
        }

        public static void AssignPlayerRole(Player player, RoleType type)
        {
            RoleAssignationRpc.Instance.Send(new Tuple<byte, byte>(player.Id, (byte)type));
            OnPlayerRoleAssigned(player, type);
        }

        public static void AssignModifierRandomly(ModifierType type, List<Player> players)
        {
            int index = AmongUsRevamped.Rand.Next(0, players.Count);
            Player player = players[index];
            AssignPlayerModifier(player, type);
        }

        public static void AssignPlayerModifier(Player player, ModifierType type)
        {
            ModifierAssignationRpc.Instance.Send(new Tuple<byte, byte>(player.Id, (byte)type));
            OnPlayerModifierAssigned(player, type);
        }

        private static void OnPlayerRoleAssigned(Player player, RoleType type)
        {
            switch (type)
            {
                case RoleType.Crewmate:
                    new Crewmate(player).AddToReverseIndex();
                    break;
                case RoleType.Sheriff:
                    new Sheriff(player).AddToReverseIndex();
                    break;
                case RoleType.TimeLord:
                    new TimeLord(player).AddToReverseIndex();
                    break;
                case RoleType.Cleaner:
                    new Cleaner(player).AddToReverseIndex();
                    break;
                case RoleType.Impostor:
                    new Impostor(player).AddToReverseIndex();
                    break;
                case RoleType.Swooper:
                    new Swooper(player).AddToReverseIndex();
                    break;
                case RoleType.Jester:
                    new Jester(player).AddToReverseIndex();
                    break;
                default:
                    AmongUsRevamped.LogWarning($"Player {player} was assigned unhandled role {type}");
                    break;
            }
        }

        private static void OnPlayerModifierAssigned(Player player, ModifierType type)
        {
            switch (type)
            {
                case ModifierType.Drunk:
                    new Drunk(player).AddToReverseIndex();
                    break;
                case ModifierType.Flash:
                    new Flash(player).AddToReverseIndex();
                    break;
                case ModifierType.Torch:
                    new Torch(player).AddToReverseIndex();
                    break;
                default:
                    AmongUsRevamped.LogWarning($"Player {player} was assigned unhandled modifier {type}");
                    break;
            }
        }

        private static void OnIntroStart(IntroCutscene introCutScene, ref Il2CppSystem.Collections.Generic.List<PlayerControl> team)
        {
            Player.OnIntroStart(introCutScene, ref team);
        }

        private static void OnIntroUpdate(IntroCutscene.Nested_0 introCutScene)
        {
            Player.OnIntroUpdate(introCutScene);
        }

        private static void OnExileBegin(Player exiled, bool tie)
        {
            exiled?.OnExiled();
        }

        private static void OnExileEnd(ExileController exileController)
        {

        }

        private static void OnEmergencyButtonUpdate(EmergencyMinigame emButton)
        {
            // Check whether role can call for a meeting or not
            Role role = Player.CurrentPlayer?.Role;
            if (role?.CanCallMeeting() ?? true) return;

            emButton.StatusText.text = $"{role.Name} can't call for a emergency meeting";
            emButton.NumberText.text = string.Empty;
            emButton.ClosedLid.gameObject.SetActive(true);
            emButton.OpenLid.gameObject.SetActive(false);
            emButton.ButtonActive = false;
        }

        private static bool OnPlayerCanUseConsole(Player player, Console console, ref float distance, out bool canUse, out bool couldUse)
        {
            canUse = couldUse = false;

            // Prevent roles faking tasks to actually complete them
            if (console.AllowImpostor || !player.FakesTasks) return true;

            distance = float.MaxValue;

            return false;
        }

        private static void OnPlayerCanUseVent(Player player, Vent vent, ref float distance, out bool canUse, out bool couldUse)
        {
            float dist = float.MaxValue;
            var usableDistance = vent.UsableDistance;
            canUse = couldUse = player?.CanUseVent(vent) ?? false;

            if (canUse)
            {
                Vector2 truePosition = player.Control.GetTruePosition();
                Vector3 position = vent.transform.position;
                dist = Vector2.Distance(truePosition, position);
                canUse &= dist <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }
            
            distance = dist;
        }

        public static void PlayerMurderedPlayer(Player killer, Player target)
        {
            if (killer == null && target == null) return;
            MurderRpc.Instance.Send(new Tuple<byte, byte>(killer.Id, target.Id));
        }

        private static void OnPlayerMurderedPlayer(Player killer, Player victim)
        {
            victim.OnMurdered(killer);
        }

        public static void RevivePlayer(Player revived)
        {
            if (revived == null) return;
            ReviveRpc.Instance.Send(revived.Id);
        }

        public static void RemoveBody(DeadBody body)
        {
            if (body == null) return;
            RemoveBodyRpc.Instance.Send(body.ParentId);
            Coroutines.Start(RemoveBodyCoroutine(body.ParentId));
        }

        private static bool OnPlayerCallMeeting(Player player)
        {
            return true;
        }

        private static void OnPlayerCalledMeeting(Player player)
        {

        }

        private static bool OnPlayerReportBody(Player player, Player victim)
        {
            return true;
        }

        private static void OnPlayerReportedBody(Player player, Player victim)
        {

        }

        private static bool OnPlayerDie(Player player)
        {

            return true;
        }

        private static void OnPlayerCompletedTask(Player player, GameData.TaskInfo task)
        {

        }

        private static bool OnPlayerExile(Player player)
        {
            return true;
        }

        private static void OnPlayerExiled(Player player)
        {

        }

        private static string GetTextForExile(Player player, StringNames id)
        {
            if (player == null) return null;

            var role = player.Role;

            // Crewmate exile
            if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN)
            {
                if (role != null) return role.ExileDescription();
                return player.Name + " was not The Impostor.";
            }

            // Impostor exile
            if (id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
            {
                if (role != null) return role.ExileDescription();
                return player.Name + " was The Impostor.";
            }

            return null;
        }
    }
}
