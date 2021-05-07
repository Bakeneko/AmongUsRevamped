using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Mod.Roles;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    public partial class Game
    {
        private static bool CheckEnd(ShipStatus shipStatus)
        {
            if (TestMode.DisableGameEnd) return false;

            // Not in a game or not hosting
            if (!GameData.Instance || !AmongUsClient.Instance.AmHost) return false;
            // Ignore tutorial
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;

            if (CheckJestersWin()) return false;
            if (CheckSabotageEnd(shipStatus)) return false;
            if (CheckImpostorsWin()) return false;
            if (CheckCompletedTasksWin()) return false;
            if (CheckCrewmatesWin()) return false;

            return false;
        }

        /// <summary>
        /// Check for jester win
        /// </summary>
        private static bool CheckJestersWin()
        {
            var winningJester = Role.GetRoles<Jester>(RoleType.Jester).FirstOrDefault(j => j.Exiled);
            if (winningJester != null)
            {
                EndGame(CustomGameOverReason.JesterVotedOut, new List<Player>(){ winningJester.Player });
                return true;
            }

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
                EndGame(CustomGameOverReason.ImpostorBySabotage, Role.AllRoles.Where(r => r.Faction == Faction.Impostors).Select(r => r.Player).ToList());
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
            // Check for reactor meltdown or seismic stabilizers failure
            systemType = shipStatus.Systems.ContainsKey(SystemTypes.Reactor) ? shipStatus.Systems[SystemTypes.Reactor] : null;
            systemType ??= (shipStatus.Systems.ContainsKey(SystemTypes.Laboratory) ? shipStatus.Systems[SystemTypes.Laboratory] : null);
            ICriticalSabotage criticalSystem = systemType?.TryCast<ICriticalSabotage>();
            if (criticalSystem?.Countdown < 0f)
            {
                EndGame(CustomGameOverReason.ImpostorBySabotage, Role.AllRoles.Where(r => r.Faction == Faction.Impostors).Select(r => r.Player).ToList());
                criticalSystem.ClearSabotage();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if crewmates should win by completing tasks
        /// </summary>
        private static bool CheckCompletedTasksWin()
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                var crewmates = Role.AllRoles.Where(r => r.Faction == Faction.Crewmates).Select(r => r.Player).ToList();
                EndGame(CustomGameOverReason.HumansByTask, crewmates);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if impostors should win by numbers
        /// </summary>
        private static bool CheckImpostorsWin()
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
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => CustomGameOverReason.ImpostorByVote,
                    DeathReason.Disconnect => CustomGameOverReason.HumansDisconnect,
                    _ => CustomGameOverReason.ImpostorByKill,
                };
                EndGame(endReason, Role.AllRoles.Where(r => r.Faction == Faction.Impostors).Select(r => r.Player).ToList());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if crewmates should win by killing all impostors
        /// </summary>
        private static bool CheckCrewmatesWin()
        {
            // Process alive impostors
            int impostors = 0;
            foreach (Player p in Player.AllPlayers)
            {
                if (!p.IsDisconnected && !p.IsDead && p.IsImpostor) impostors++;
            }

            if (impostors == 0)
            {
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Disconnect => CustomGameOverReason.ImpostorDisconnect,
                    _ => CustomGameOverReason.HumansByVote,
                };
                EndGame(endReason, Role.AllRoles.Where(r => r.Faction == Faction.Crewmates).Select(r => r.Player).ToList());
                return true;
            }

            return false;
        }

        public static void EndGame(CustomGameOverReason reason, List<Player> winners)
        {
            if (ShipStatus.Instance != null)
            {
                GameOver = new GameOverData(reason, winners);
                EndGameRpc.Instance.Send(new EndGameRpc.GameOver(reason, winners), true);
                ShipStatus.RpcEndGame((GameOverReason)reason, false);
                ShipStatus.Instance.enabled = false;
            }
        }

        /// <summary>
        /// Game ended, process winners and loosers
        /// </summary>
        private static void OnEnd(AmongUsClient client, GameOverData gameOver)
        {
            Player.CurrentPlayer?.OnEnd(gameOver);
        }

        /// <summary>
        /// Game is over, display game over screen
        /// </summary>
        private static void OnGameOver(EndGameManager endGameManager)
        {
            if (GameOver == null) return;

            GameObject bonusText = Object.Instantiate(endGameManager.WinText.gameObject);
            bonusText.transform.position = new Vector3(endGameManager.WinText.transform.position.x, endGameManager.WinText.transform.position.y - 0.8f, endGameManager.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text text = bonusText.GetComponent<TMPro.TMP_Text>();
            text.text = string.Empty;
            text.color = Color.white;

            endGameManager.WinText.text = GameOver.Title ?? endGameManager.WinText.text;
            endGameManager.WinText.color = GameOver.TitleColor != Color.black ? GameOver.TitleColor : endGameManager.WinText.color;
            text.text = GameOver.Text;
            text.color = GameOver.TextColor != Color.black ? GameOver.TextColor : text.color;
            endGameManager.BackgroundBar.material.color = GameOver.BackgroundColor != Color.black ? GameOver.BackgroundColor
                : endGameManager.BackgroundBar.material.color;
        }

        private static bool IsGameOverDueToDeath()
        {
            return false;
        }

        public enum CustomGameOverReason
        {
            HumansByVote,
            HumansByTask,
            ImpostorByVote,
            ImpostorByKill,
            ImpostorBySabotage,
            ImpostorDisconnect,
            HumansDisconnect,
            JesterVotedOut
        }

        public class GameOverData
        {
            public CustomGameOverReason Reason;
            public List<Player> Winners;
            public string Title;
            public string Text;
            public Color TitleColor = Color.black;
            public Color TextColor = Color.black;
            public Color BackgroundColor = Color.black;

            public GameOverData(CustomGameOverReason reason, List<Player> winners = null)
            {
                Reason = reason;
                Winners = winners;
            }

            public static implicit operator GameOverReason(GameOverData gameOver) => (GameOverReason)gameOver.Reason;
        }
    }
}
