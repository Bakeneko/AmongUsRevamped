using System;
using System.Collections.Generic;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using Hazel;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class TimeLord : Crewmate
    {
        private CooldownButton RewindButton = null;
        private AudioClip RewindSound = null;

        private readonly float RecordLimit = Options.Values.TimeLordRewindDuration;
        private readonly int RewindSpeed = 3;
        private readonly List<Tuple<Vector3, Vector2, float>> Records = new();
        private bool RecordEnabled = false;
        private bool Rewinding = false;

        private float DeathTime = -1;
        private Color HudColor = Color.clear;

        public TimeLord(Player player) : base(player, RoleType.TimeLord)
        {
            Name = "Time Lord";
            Color = ColorPalette.Color.RoleTimeLord;
            IntroDescription = () => $"Rewind!";
            TaskDescription = () => Color.ToColorTag($"{Name}: Save crewmates by rewinding time");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Init();
        }

        protected void Init()
        {
            if (Player.IsCurrentPlayer)
            {
                RewindButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_timelord_rewind.png", new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight), 
                    Options.Values.TimeLordRewindCooldown, Options.Values.TimeLordRewindDuration / RewindSpeed, Options.Values.TimeLordRewindDuration)
                {
                    HotKey = KeyCode.Q,
                    Clickable = false,
                    Visible = true
                };
                RewindButton.Clicked += OnRewindButtonClicked;
                RewindButton.ApplyCooldown(RewindButton.InitialCooldownDuration);
            }

            RewindSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_timelord_rewind.wav");
        }

        public override void OnIntroEnd(IntroCutscene introCutScene)
        {
            base.OnIntroEnd(introCutScene);
            RecordEnabled = true;
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
            if (Rewinding) Rewind(); else Record();
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);

            if (RewindButton != null)
            {
                RewindButton.Visible = !Player.Dead;
                RewindButton.Clickable = Player.CanMove;
            }
        }

        protected void OnRewindButtonClicked(object sender, EventArgs e)
        {
            RewindRpc.Instance.Send(0, true);
            StartRewind();
        }

        protected void StartRewind()
        {
            var speed = RewindSound.length / (Math.Max(Records.Count, 1) / RewindSpeed * Time.deltaTime);
            SoundManager.Instance.PlaySound(RewindSound, false, 0.6f).pitch = speed;
            Rewinding = true;
            PlayerControl.LocalPlayer.moveable = false;
            HudColor = HudManager.Instance.FullScreen.color;
            HudManager.Instance.FullScreen.color = ColorPalette.Color.RewindHudColor;
            HudManager.Instance.FullScreen.enabled = true;
        }

        protected void EndRewind()
        {
            Rewinding = false;
            PlayerControl.LocalPlayer.moveable = true;
            HudManager.Instance.FullScreen.color = HudColor;
            HudManager.Instance.FullScreen.enabled = false;
        }

        protected void Record()
        {
            if (!RecordEnabled) return;
            
            var player = Player.CurrentPlayer;

            if (player == null) return;

            var limit = (int)Mathf.Round(RecordLimit / Time.deltaTime);
            if (Records.Count > limit)
            {
                Records.RemoveRange(limit, Records.Count - limit);
            }

            Vector3 position = player.Control.transform.position;
            Vector2 velocity = player.Control.MyPhysics.body.velocity;

            if (DeathTime > 0 && !player.Dead)
            {
                DeathTime = -1;
            }
            else if (DeathTime < 0 && player.Dead)
            {
                DeathTime = Time.time;
            }

            if (!player.CanMove)
            {
                if (Records.Count > 0)
                {
                    position = Records[0].Item1;
                    velocity = Vector2.zero;
                }
                else return;
            }

            Records.Insert(0, new Tuple<Vector3, Vector2, float>(position, velocity, Time.time));
        }

        protected void Rewind()
        {
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                }
                catch { }
            }

            if (Records.Count == 0)
            {
                EndRewind();
                return;
            }

            // Skip records to speed up the process but never skip the last
            for (int i = 0; i < RewindSpeed - 1; i++)
            {
                if (Records.Count > 1)
                {
                    Records.RemoveAt(0);
                }
            }

            //PlayerControl.LocalPlayer.Physics.ExitAllVents
            var player = Player.CurrentPlayer;

            // Exit vent if necessary
            if (player.Control.inVent)
            {
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    vent.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool couldUse);
                    if (canUse)
                    {
                        player.Control.MyPhysics.RpcExitVent(vent.Id);
                        vent.SetButtons(false);
                        break;
                    }
                }
            }

            // Fix animation
            if (!player.Control.Collider.enabled)
            {
                player.Control.MyPhysics.ResetMoveState(true);
                player.Control.Collider.enabled = true;
                player.Control.moveable = true;
                player.Control.NetTransform.enabled = true;

                RewindRpc.Instance.Send(1, true);
            }

            var rec = Records[0];

            player.Control.transform.position = rec.Item1;
            player.Control.MyPhysics.body.velocity = rec.Item2 * RewindSpeed;

            if (rec.Item3 <= DeathTime && player.Dead)
            {
                player.Revive();
                DeathTime = -1;
            }

            Records.RemoveAt(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    RewindButton?.Dispose();
                    RewindButton = null;

                    RewindSound?.Destroy();
                    RewindSound = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.Rewind)]
        private protected class RewindRpc : PlayerCustomRpc<byte>
        {
            public static RewindRpc Instance { get { return Rpc<RewindRpc>.Instance; } }

            public RewindRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, byte msg)
            {
                writer.Write(msg); // 0: rewind start, 1: animation fix
            }

            public override byte Read(MessageReader reader)
            {
                return reader.ReadByte();
            }

            public override void Handle(PlayerControl sender, byte msg)
            {
                var originPlayer = Player.GetPlayer(sender?.PlayerId ?? -1);

                switch(msg)
                {
                    case 0: // Rewind start
                        var timeLord = GetPlayerRole<TimeLord>(originPlayer?.Id ?? -1);
                        timeLord?.StartRewind();
                        break;
                    case 1: // Animation fix
                        originPlayer?.FixAnimation();
                        break;
                }
            }
        }
    }
}
