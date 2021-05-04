using System;
using System.ComponentModel;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Impostor : Role
    {
        public CooldownButton KillButton = null;
        public Player CurrentTarget { get; set; }

        public Impostor(Player player) : base(player)
        {
            Name = "Impostor";
            Faction = Faction.Impostors;
            RoleType = RoleType.Impostor;
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => "Sabotage and kill everyone";
            TaskDescription = () => Color.ToColorTag($"{Name}: Sabotage and kill everyone");
            ExileDescription = () => $"{Player.Name} was an {Name}";
            Init();
        }

        public void Init()
        {
            CurrentTarget = null;
            if (Player.IsCurrentPlayer)
            {
                var defaultButton = HudManager.Instance.KillButton;
                defaultButton.gameObject.SetActive(false);
                defaultButton.renderer.enabled = false;
                defaultButton.isActive = false;
                defaultButton.enabled = false;
                KillButton = new CooldownButton((Sprite)null, new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight), PlayerControl.GameOptions.KillCooldown, 0f, 10f);
                KillButton.HotKey = KeyCode.Q;
                KillButton.Clickable = false;
                KillButton.Visible = true;
                KillButton.Clicked += OnKillButtonClicked;
                KillButton.ApplyCooldown(KillButton.InitialCooldownDuration);
            }
        }

        protected internal override bool CanUseVent(Vent vent)
        {
            return true;
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            if (Player.IsDead)
            {
                if (KillButton != null) KillButton.Visible = false;
                return;
            }

            CurrentTarget = SearchTarget();
            if (CurrentTarget == null)
            {
                if (KillButton != null) KillButton.Clickable = false;
                return;
            }

            if (KillButton != null) KillButton.Clickable = true;
            CurrentTarget.SetOutline(Color);
        }

        private Player SearchTarget()
        {
            // Abort!
            if (!ShipStatus.Instance || Player.IsDead || !Player.Control.CanMove) return null;
            PlayerControl target = null;

            float distance = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            Vector2 truePosition = Player.Control.GetTruePosition();
            var players = PlayerControl.AllPlayerControls;
            foreach (PlayerControl p in players)
            {
                // Don't want to target those
                if (p.Data.Disconnected || p.Data.IsDead || p.PlayerId == Player.Id || p.Data.IsImpostor || p.inVent) continue;

                Vector2 vector = p.GetTruePosition() - truePosition;
                float magnitude = vector.magnitude;
                // Check if nearest and no obtruction
                if (magnitude <= distance && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                {
                    target = p;
                    distance = magnitude;
                }
            }

            return target;
        }

        public void OnKillButtonClicked(object sender, EventArgs e)
        {
            Player.MurderPlayer(CurrentTarget);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    CurrentTarget = null;
                    if (KillButton != null)
                    {
                        KillButton.Dispose();
                    }
                    KillButton = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
            Disposed = true;
        }
    }
}
