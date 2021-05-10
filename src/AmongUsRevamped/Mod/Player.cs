using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Mod.Modifiers;
using AmongUsRevamped.Mod.Roles;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    public class Player
    {

        public static IEnumerable<Player> AllPlayers => PlayerControl.AllPlayerControls.ToArray().Select(p => new Player(p));

        public static Player CurrentPlayer => PlayerControl.LocalPlayer;

        public static Player GetPlayer(int id)
        {
            return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == id);
        }

        public PlayerControl Control { get; }

        public Player(PlayerControl player)
        {
            Control = player;
        }

        public bool IsCurrentPlayer => Control == PlayerControl.LocalPlayer;

        public byte Id => Control.PlayerId;
        public GameData.PlayerInfo Data => Control.Data;
        public string Name => Data?.PlayerName;
        public bool IsDisconnected => Data.Disconnected;
        public bool IsDead => Data.IsDead;
        public bool IsImpostor => Data.IsImpostor;
        public bool Visible { get => Control.Visible; set => Control.Visible = value; }
        public bool CanMove => Control.CanMove;

        public float MoveSpeed => IsDead ? 1f : (Role?.MoveSpeed ?? 1f) * (Modifier?.MoveSpeedModifier ?? 1f);
        public float VisionRange => (Role?.VisionRange ?? PlayerControl.GameOptions.CrewLightMod) * (Modifier?.VisionRangeModifier ?? 1f);
        public bool HasNightVision => Role?.HasNightVision == true || Modifier?.HasNightVision == true;
        public bool FakesTasks => Role?.FakesTasks == true;

        public Role Role => Role.GetPlayerRole(Id);
        public Modifier Modifier => Modifier.GetPlayerModifier(Id);

        public IEnumerable<GameData.TaskInfo> Tasks => Data.Tasks.ToArray();

        public bool CanSeeRole(Player other)
        {
            return (IsDead && Options.Values.GhostsSeeRoles) || (Role?.CanSeeRole(other) ?? false);
        }

        public bool CanUseVent(Vent vent)
        {
            return !IsDead && (Control.inVent || (Control.CanMove && (Role?.CanUseVent(vent) ?? false)));
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnFixedUpdate()
        {
            // Not playing
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

            if (IsCurrentPlayer)
            {
                UpdatePlayerTextInfo(this, true, Options.Values.DisplayTasks);
            }
            else if (CurrentPlayer.IsDead)
            {
                UpdatePlayerTextInfo(this, Options.Values.GhostsSeeRoles, Options.Values.GhostsSeeTasks);
            }
        }

        public virtual void OnExiled()
        {
            Role?.OnExiled();
            if (IsCurrentPlayer && FakesTasks) ClearTasks();
        }

        public virtual void OnMurdered(Player killer)
        {
            Role?.OnMurdered(killer);
            if (IsCurrentPlayer)
            {
                UpdateImportantTasks();
                if (FakesTasks) ClearTasks();
            }
        }

        public virtual void OnRevived()
        {
            Control.Revive();
            var body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                .FirstOrDefault(b => b.ParentId == Id);
            body?.gameObject.Destroy();

            Role?.OnRevived();

            if (IsCurrentPlayer)
            {
                Control.myTasks.RemoveAt(0);
                UpdateImportantTasks();
            }
        }

        public virtual void OnEnd(Game.GameOverData gameOver)
        {
            Role?.OnEnd(gameOver);
        }

        public void UpdateImportantTasks()
        {
            if (!IsCurrentPlayer) return;

            var delete = new List<PlayerTask>();

            if (IsDead)
            {
                // Remove role and modifier tasks
                foreach (PlayerTask t in Control.myTasks)
                {
                    if (t.name.Equals("RoleTask") || t.name.Equals("ModifierTask"))
                    {
                        delete.Add(t);
                    }
                }
                foreach (PlayerTask t in delete)
                {
                    t.OnRemove();
                    Control.myTasks.Remove(t);
                    UnityEngine.Object.Destroy(t.gameObject);
                }

                return;
            }

            var modifierText = Modifier?.TaskDescription();
            var roleText = Role?.TaskDescription();

            // Remove unwanted tasks
            foreach (PlayerTask t in Control.myTasks)
            {
                var task = t.gameObject.GetComponent<ImportantTextTask>();
                if (task != null)
                {
                    if (t.name.Equals("RoleTask") && task.Text.Equals(roleText)) roleText = null;
                    else if (t.name.Equals("ModifierTask") && task.Text.Equals(modifierText)) modifierText = null;
                    else delete.Add(t);
                }
            }
            foreach (PlayerTask t in delete)
            {
                t.OnRemove();
                Control.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add role task if needed
            if (roleText != null)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                Control.myTasks.Insert(0, task);
                task.transform.SetParent(Control.transform, false);
                task.Text = roleText;
            }

            // Add modifier task if needed
            if (modifierText != null)
            {
                var task = new GameObject("ModifierTask").AddComponent<ImportantTextTask>();
                Control.myTasks.Insert(1, task);
                task.transform.SetParent(Control.transform, false);
                task.Text = modifierText;
            }
        }

        public void ClearTasks()
        {
            for (int i = 0; i < Control.myTasks.Count; i++)
            {
                PlayerTask playerTask = Control.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            Control.myTasks.Clear();

            if (Control.Data != null && Control.Data.Tasks != null)
                Control.Data.Tasks.Clear();
        }

        /// <summary>
        /// Process completed task
        /// </summary>
        public Tuple<short, short> GetTasksStatus()
        {
            return GetTasksStatus(Data);
        }

        public override string ToString()
        {
            return $"{{Id: {Id}, Name: \"{Name}\"}}";
        }

        public static implicit operator Player(GameData.PlayerInfo playerInfo)
        {
            return playerInfo?.Object;
        }

        public static implicit operator Player(PlayerControl playerControl)
        {
            return playerControl != null ? new Player(playerControl) : null;
        }

        /// <summary>
        /// Process completed tasks for a given player
        /// </summary>
        public static Tuple<short, short> GetTasksStatus(GameData.PlayerInfo player)
        {
            short completedTasks = 0;
            short totalTasks = 0;

            var tasks = player.Tasks;
            if (!player.Disconnected && tasks != null &&
                (!player.IsDead || PlayerControl.GameOptions.GhostsDoTasks) &&
                Role.GetPlayerRole(player.PlayerId)?.FakesTasks != true
                )
            {
                foreach (GameData.TaskInfo task in tasks)
                {
                    totalTasks++;
                    if (task.Complete) completedTasks++;
                }
            }

            return new Tuple<short, short>(completedTasks, totalTasks);
        }

        public virtual void HudUpdate(HudManager hudManager)
        {
            // Reset outline
            SetOutline(null);
            Role?.HudUpdate(hudManager);
            Modifier?.HudUpdate(hudManager);
        }

        public virtual void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            Role?.CurrentPlayerHudUpdate(hudManager);
            Modifier?.CurrentPlayerHudUpdate(hudManager);
        }

        public virtual void MurderPlayer(Player victim)
        {
            if (victim != null)
            {
                Game.PlayerMurderedPlayer(this, victim);
                Control.MurderPlayer(victim.Control);
            }
        }

        public void Revive()
        {
            Game.RevivePlayer(this);
            OnRevived();
        }

        public void SetOutline(Color? color)
        {
            Control?.myRend?.SetOutline(color);
        }

        public void SetSkin(int skindId)
        {
            if (Control == null) return;

            SkinData nextSkin = DestroyableSingleton<HatManager>.Instance.AllSkins[skindId];
            var physics = Control.MyPhysics;

            AnimationClip clip;
            var spriteAnim = physics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = physics.Skin;

            var currentPhysicsAnim = physics.Animator.GetCurrentAnimation();

            if (currentPhysicsAnim == physics.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == physics.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == physics.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == physics.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == physics.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            float progress = physics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }

        public void FixAnimation()
        {
            Control.MyPhysics.ResetMoveState(true);
            Control.Collider.enabled = true;
            Control.moveable = true;
            Control.NetTransform.enabled = true;
        }

        public static void OnIntroStart(IntroCutscene introCutScene, ref Il2CppSystem.Collections.Generic.List<PlayerControl> team)
        {
            Player player = CurrentPlayer;
            player?.Role?.OnIntroStart(introCutScene, ref team);
            player?.Modifier?.OnIntroStart(introCutScene);
        }

        public static void OnIntroUpdate(IntroCutscene.Nested_0 introCutScene)
        {
            Player player = CurrentPlayer;
            player?.Role?.OnIntroUpdate(introCutScene);
            player?.Modifier?.OnIntroUpdate(introCutScene);
        }

        public static void UpdatePlayerTextInfos()
        {
            foreach (Player player in AllPlayers)
            {
                if (player.IsCurrentPlayer)
                {
                    UpdatePlayerTextInfo(player, true, Options.Values.DisplayTasks);
                }
                else if (CurrentPlayer.IsDead)
                {
                    UpdatePlayerTextInfo(player, Options.Values.GhostsSeeRoles, Options.Values.GhostsSeeTasks);
                }
            }
        }

        public static void UpdatePlayerTextInfo(Player player, bool showRole, bool showTasks)
        {
            if (player == null) return;

            // Retrieve or instantiate player text
            Transform playerInfoTransform = player.Control.transform?.FindChild("PlayerInfo");
            TMPro.TextMeshPro playerInfo = playerInfoTransform?.GetComponent<TMPro.TextMeshPro>();
            if (playerInfo == null)
            {
                playerInfo = UnityEngine.Object.Instantiate(player.Control.nameText, player.Control.nameText.transform.parent);
                playerInfo.transform.localPosition += Vector3.up * 0.25f;
                playerInfo.fontSize *= 0.75f;
                playerInfo.gameObject.name = "PlayerInfo";
            }
            // Retrieve or instantiate player meeting text
            PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == player.Id);
            Transform meetingInfoTransform = playerVoteArea != default ? playerVoteArea.transform?.FindChild("PlayerInfo") : null;
            TMPro.TextMeshPro meetingInfo = meetingInfoTransform?.GetComponent<TMPro.TextMeshPro>();
            if (meetingInfo == null && playerVoteArea != null)
            {
                meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * (MeetingHud.Instance.playerStates.Length > 10 ? 0.4f : 0.25f);
                meetingInfo.fontSize *= 0.75f;
                meetingInfo.gameObject.name = "PlayerInfo";
            }

            string roleInfo = "";
            if (showRole)
            {
                var role = player.Role;
                if (role != null) roleInfo = role.Color.ToColorTag(role.Name);
            }

            string tasksInfo = "";
            if (showTasks)
            {
                var (completedTasks, totalTasks) = player.GetTasksStatus();
                Color32 tasksColor = completedTasks > 0 ?
                    (completedTasks < totalTasks ? ColorPalette.Color.TasksIncomplete : ColorPalette.Color.TasksComplete) :
                    Color.white;

                tasksInfo = totalTasks > 0 ? tasksColor.ToColorTag($"({completedTasks}/{totalTasks})") : "";
            }

            string info = $"{roleInfo} {tasksInfo}".Trim();

            playerInfo.text = info;
            playerInfo.gameObject.SetActive(player.Visible);
            if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : info;
        }
    }
}
