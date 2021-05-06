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
        public static float BasePlayerSpeed = AmongUsClient.Instance?.PlayerPrefab?.MyPhysics?.Speed ?? 2.5f;

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

            pingTracker.text.text = ColorPalette.Color.Revamped.ToColorTag($"{AmongUsRevamped.Name} v{AmongUsRevamped.Version}");
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

            for (int i = 0; i < maxCrewmateRoles; i++)
            {
                if (generator.GetNumberCount() > 0)
                {
                    var role = generator.GetDistributedRandomNumber();
                    generator.RemoveNumber(role);
                    AssignRoleRandomly((RoleType)role, crewmates);
                }
                else // No more special roles, assign base crewmate role
                {
                    break;
                }
            }
            crewmates.ForEach(p => AssignPlayerRole(p, RoleType.Crewmate));

            int maxImpostorRoles = Mathf.Min(impostors.Count, Options.Values.MaxImpostorRoles);
            generator = new DistributedRandomNumberGenerator<byte>();

            for (int i = 0; i < maxImpostorRoles; i++)
            {
                if (generator.GetNumberCount() > 0)
                {
                    var role = generator.GetDistributedRandomNumber();
                    generator.RemoveNumber(role);
                    AssignRoleRandomly((RoleType)role, impostors);
                }
                else // No more special roles, assign base impostor role
                {
                    break;
                }
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
                    new Crewmate(player);
                    break;
                case RoleType.Sheriff:
                    new Sheriff(player);
                    break;
                case RoleType.Impostor:
                    new Impostor(player);
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
                    new Drunk(player);
                    break;
                case ModifierType.Flash:
                    new Flash(player);
                    break;
                case ModifierType.Torch:
                    new Torch(player);
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

        private static void OnExileBegin()
        {

        }

        private static void OnExileEnd()
        {

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

        public static void PlayerMurderedPlayer(Player player, Player target)
        {
            if (player == null && target == null) return;
            MurderRpc.Instance.Send(new Tuple<byte, byte>(player.Id, target.Id));
        }

        private static void OnPlayerMurderedPlayer(Player player, Player victim)
        {
            if (victim.IsCurrentPlayer) victim.UpdateImportantTasks();
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

        private static bool CheckEnd(ShipStatus shipStatus)
        {
            // Not in a game or not hosting
            if (!GameData.Instance || !AmongUsClient.Instance.AmHost) return false;
            // Ignore tutorial
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;

            if (CheckSabotageEnd(shipStatus)) return false;
            if (CheckImpostorsWin(shipStatus)) return false;
            if (CheckCompletedTasksWin(shipStatus)) return false;
            if (CheckCrewmatesWin(shipStatus)) return false;

            return false;
        }

        /// <summary>
        /// Check if impostors should win by critical sabotage
        /// </summary>
        private static bool CheckSabotageEnd(ShipStatus shipStatus)
        {
            if (shipStatus.Systems == null) return false;

            // Check life support failure
            ISystemType systemType = shipStatus.Systems.ContainsKey(SystemTypes.LifeSupp) ? shipStatus.Systems[SystemTypes.LifeSupp] : null;
            LifeSuppSystemType lifeSuppSystemType = systemType?.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType?.Countdown < 0f)
            {
                shipStatus.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
            // Check for reactor meltdown or seismic stabilizers failure
            systemType = shipStatus.Systems.ContainsKey(SystemTypes.Reactor) ? shipStatus.Systems[SystemTypes.Reactor] : null;
            systemType ??= (shipStatus.Systems.ContainsKey(SystemTypes.Laboratory) ? shipStatus.Systems[SystemTypes.Laboratory] : null);
            ICriticalSabotage criticalSystem = systemType?.TryCast<ICriticalSabotage>();
            if (criticalSystem?.Countdown < 0f)
            {
                shipStatus.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
                criticalSystem.ClearSabotage();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if crewmates should win by completing tasks
        /// </summary>
        private static bool CheckCompletedTasksWin(ShipStatus shipStatus)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                shipStatus.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if impostors should win by numbers
        /// </summary>
        private static bool CheckImpostorsWin(ShipStatus shipStatus)
        {
            // Process alive players
            int total = 0, impostors = 0;
            foreach (Player p in Player.AllPlayers)
            {
                if (p.IsDisconnected || p.IsDead) continue;
                total++;
                if (p.IsImpostor) impostors++;
            }

            if (impostors >= total - impostors)
            {
                shipStatus.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Disconnect => GameOverReason.HumansDisconnect,
                    _ => GameOverReason.ImpostorByKill,
                };
                ShipStatus.RpcEndGame(endReason, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if crewmates should win by killing all impostors
        /// </summary>
        private static bool CheckCrewmatesWin(ShipStatus shipStatus)
        {
            // Process alive impostors
            int impostors = 0;
            foreach (Player p in Player.AllPlayers)
            {
                if (!p.IsDisconnected && !p.IsDead && p.IsImpostor) impostors++;
            }

            if (impostors == 0)
            {
                shipStatus.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Disconnect => GameOverReason.ImpostorDisconnect,
                    _ => GameOverReason.HumansByVote,
                };
                ShipStatus.RpcEndGame(endReason, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Game ended, process winners and loosers
        /// </summary>
        private static void OnEnd(AmongUsClient client, GameOverReason reason)
        {

        }

        /// <summary>
        /// Game is over, display game over screen
        /// </summary>
        private static void OnGameOver(EndGameManager endGameManager)
        {

        }

        private static bool IsGameOverDueToDeath()
        {
            return false;
        }
    }
}
