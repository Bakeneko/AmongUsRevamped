using System;
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

        public Impostor(Player player, RoleType roleType = RoleType.Impostor) : base(player, roleType)
        {
            Name = "Impostor";
            Faction = Faction.Impostors;
            RoleType = RoleType.Impostor;
            Color = ColorPalette.Color.RoleImpostor;
            VisionRange = PlayerControl.GameOptions.ImpostorLightMod;
            HasNightVision = true;
            FakesTasks = true;
            IntroDescription = () => "Sabotage and kill everyone";
            TaskDescription = () => Color.ToColorTag($"{Name}: Sabotage and kill everyone");
            ExileDescription = () => $"{Player.Name} was an {Name}";
            Player.Data.IsImpostor = true;
            Init();
        }

        protected virtual void Init()
        {
            CurrentTarget = null;
            if (Player.IsCurrentPlayer)
            {
                var defaultButton = HudManager.Instance.KillButton;
                defaultButton.gameObject.SetActive(false);
                defaultButton.renderer.enabled = false;
                defaultButton.isActive = false;
                defaultButton.enabled = false;
                KillButton = new CooldownButton((Sprite)null, new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight), PlayerControl.GameOptions.KillCooldown, 0f, 10f)
                {
                    HotKey = KeyCode.Q,
                    Clickable = false,
                    Visible = true
                };
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
            base.CurrentPlayerHudUpdate(hudManager);
            CurrentTarget = SearchTarget();

            if (KillButton != null)
            {
                KillButton.Visible = !Player.Dead;
                KillButton.Clickable = CurrentTarget != null && Player.CanMove;
            }

            CurrentTarget?.SetOutline(Color);
        }

        protected Player SearchTarget()
        {
            // Abort!
            if (!ShipStatus.Instance || Player.Dead || !Player.Control.CanMove) return null;
            PlayerControl target = null;

            bool spyAmongUs = GetRoles(RoleType.Spy).Count > 0; // Are other impostors sus?

            float distance = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            Vector2 truePosition = Player.Control.GetTruePosition();
            var players = PlayerControl.AllPlayerControls;
            foreach (PlayerControl p in players)
            {
                // Don't want to target those
                if (p.Data.Disconnected || p.Data.IsDead || p.PlayerId == Player.Id || (p.Data.IsImpostor && !spyAmongUs)) continue;

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

        protected virtual void OnKillButtonClicked(object sender, EventArgs e)
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
                    KillButton?.Dispose();
                    KillButton = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }
    }
}
