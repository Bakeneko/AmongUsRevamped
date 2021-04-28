using System;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Mod
{
    public class PlayerInfo
    {
        public static GameData.PlayerInfo Get(int id)
        {
            return GameData.Instance.AllPlayers.ToArray().FirstOrDefault(pi => pi.PlayerId == id);
        }

        public static void UpdatePlayerTextInfo(PlayerControl player, bool showTasks)
        {
            // Retrieve or instantiate player text
            Transform playerInfoTransform = player.transform.FindChild("PlayerInfo");
            TMPro.TextMeshPro playerInfo = playerInfoTransform?.GetComponent<TMPro.TextMeshPro>();
            if (playerInfo == null)
            {
                playerInfo = Object.Instantiate(player.nameText, player.nameText.transform.parent);
                playerInfo.transform.localPosition += Vector3.up * 0.25f;
                playerInfo.fontSize *= 0.75f;
                playerInfo.gameObject.name = "PlayerInfo";
            }

            // Retrieve or instantiate player meeting text
            PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            Transform meetingInfoTransform = playerVoteArea?.transform.FindChild("PlayerInfo");
            TMPro.TextMeshPro meetingInfo = meetingInfoTransform?.GetComponent<TMPro.TextMeshPro>();
            if (meetingInfo == null && playerVoteArea != null)
            {
                meetingInfo = Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * (MeetingHud.Instance.playerStates.Length > 10 ? 0.4f : 0.25f);
                meetingInfo.fontSize *= 0.75f;
                meetingInfo.gameObject.name = "PlayerInfo";
            }

            var (completedTasks, totalTasks) = TaskInfo.GetPlayerTasksStatus(player.Data);
            Color32 tasksColor = completedTasks > 0 ?
                (completedTasks < totalTasks ? ColorPalette.Color.TasksIncomplete : ColorPalette.Color.TasksComplete) :
                Color.white;

            string taskInfo = totalTasks > 0 ? tasksColor.ToColorTag($"({completedTasks}/{totalTasks})") : "";

            string info = "";
            if (showTasks)
            {
                info = taskInfo;
            }

            playerInfo.text = info;
            playerInfo.gameObject.SetActive(player.Visible);
            if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : info;
        }
    }
}
