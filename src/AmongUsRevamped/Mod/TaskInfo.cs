using System;
using HarmonyLib;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class TaskInfo
    {
        public static Tuple<short, short> GetPlayerTasksStatus(GameData.PlayerInfo player)
        {
            short completedTasks = 0;
            short totalTasks = 0;

            if (!player.Disconnected && player.Object && player.Tasks != null &&
                (!player.IsDead || PlayerControl.GameOptions.GhostsDoTasks) &&
                !player.IsImpostor
                )
            {
                foreach (GameData.TaskInfo task in player.Tasks)
                {
                    totalTasks++;
                    if (task.Complete) completedTasks++;
                }
            }

            return new Tuple<short, short>(completedTasks, totalTasks);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static bool GameDataRecomputeTaskCountsPatch(GameData __instance)
        {
            __instance.TotalTasks = 0;
            __instance.CompletedTasks = 0;
            foreach(GameData.PlayerInfo player in __instance.AllPlayers)
            {
                var (playerCompleted, playerTotal) = GetPlayerTasksStatus(player);
                __instance.TotalTasks += playerTotal;
                __instance.CompletedTasks += playerCompleted;
            }

            return false;
        }
    }
}
