using System;

namespace AmongUsRevamped.Mod
{
    public partial class Game
    {
        private static void OnPlayerTasksCreated(Player player)
        {
            if (player.IsCurrentPlayer) player.UpdateImportantTasks();
        }

        /// <summary>
        /// Process game task count
        /// </summary>
        private static void RecomputeTaskCount()
        {
            var gameData = GameData.Instance;
            gameData.TotalTasks = 0;
            gameData.CompletedTasks = 0;
            foreach (GameData.PlayerInfo player in gameData.AllPlayers)
            {
                var (playerCompleted, playerTotal) = Player.GetTasksStatus(player);
                gameData.TotalTasks += playerTotal;
                gameData.CompletedTasks += playerCompleted;
            }
        }
    }
}
