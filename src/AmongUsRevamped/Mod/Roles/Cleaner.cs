using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Cleaner : Impostor
    {
        public CooldownButton CleanButton = null;
        public AudioClip CleanSound = null;
        public DeadBody CurrentBodyTarget { get; set; }

        public Cleaner(Player player) : base(player)
        {
            Name = "Cleaner";
            RoleType = RoleType.Cleaner;
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => "Clean up bodies";
            TaskDescription = () => Color.ToColorTag($"{Name}: Clean bodies before crewmates discover them");
            ExileDescription = () => $"{Player.Name} was The {Name}";
        }

        protected override void Init()
        {
            base.Init();
            if (Player.IsCurrentPlayer)
            {
                CleanButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_cleaner_clean.png", new HudPosition(GameButton.ButtonSize, GameButton.ButtonSize, HudAlignment.BottomRight), PlayerControl.GameOptions.KillCooldown, 0f, 10f)
                {
                    HotKey = KeyCode.F,
                    Clickable = true,
                    Visible = true
                };
                CleanButton.Clicked += OnCleanButtonClicked;
                CleanButton.ApplyCooldown(CleanButton.InitialCooldownDuration);

                CleanSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_cleaner_clean.wav");
            }
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);
            if (Player.IsDead)
            {
                if (CleanButton != null) CleanButton.Visible = false;
            }

            // Reset body outline
            CurrentBodyTarget?.GetComponent<SpriteRenderer>()?.SetOutline(null);

            CurrentBodyTarget = SearchBodyTarget();
            if (CurrentBodyTarget == null)
            {
                if (CleanButton != null) CleanButton.Clickable = false;
                return;
            }

            if (CleanButton != null) CleanButton.Clickable = true;

            CurrentBodyTarget.GetComponent<SpriteRenderer>()?.SetOutline(Color);
        }

        protected DeadBody SearchBodyTarget()
        {
            // Abort!
            if (!ShipStatus.Instance || Player.IsDead || !Player.Control.CanMove) return null;
            DeadBody target = null;

            Vector2 position = Player.Control.GetTruePosition();
            float distance = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(position, distance, Constants.PlayersOnlyMask))
            {
                // We are only interested in dead bodies :)
                DeadBody body = collider2D.GetComponent<DeadBody>();
                if (body == null || body.Reported) continue;
                
                // Check distance
                Vector2 bodyPos = body.TruePosition;
                float dist = Vector2.Distance(position, bodyPos);
                if (dist <= distance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(position, bodyPos, Constants.ShipAndObjectsMask, false))
                {
                    target = body;
                    distance = dist;
                }
            }

            return target;
        }

        protected override void OnKillButtonClicked(object sender, EventArgs e)
        {
            CleanButton.ApplyCooldown();
            base.OnKillButtonClicked(sender, e);
        }

        protected void OnCleanButtonClicked(object sender, EventArgs e)
        {
            KillButton.ApplyCooldown();
            SoundManager.Instance.PlaySound(CleanSound, false, 1.0f);
            Game.RemoveBody(CurrentBodyTarget);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    CurrentBodyTarget = null;
                    CleanButton?.Dispose();
                    CleanButton = null;

                    CleanSound?.Destroy();
                    CleanSound = null;
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
