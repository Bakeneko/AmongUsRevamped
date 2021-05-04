﻿using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Sheriff : Crewmate
    {
        public CooldownButton KillButton = null;
        public Player CurrentTarget { get; set; }

        public Sheriff(Player player) : base(player)
        {
            Name = "Sheriff";
            RoleType = RoleType.Sheriff;
            Color = ColorPalette.Color.RoleSheriff;
            IntroDescription = () => $"Shoot the {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")}";
            TaskDescription = () => Color.ToColorTag($"{Name}: Shoot the Impostors");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Init();
        }

        public void Init()
        {
            CurrentTarget = null;
            if (Player.IsCurrentPlayer)
            {
                KillButton = new CooldownButton((Sprite)null, new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight), Options.Values.SheriffKillCooldown, 0f, 10f);
                KillButton.HotKey = KeyCode.Q;
                KillButton.Clickable = false;
                KillButton.Visible = true;
                KillButton.Clicked += OnKillButtonClicked;
                KillButton.ApplyCooldown(KillButton.InitialCooldownDuration);
            }
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
                if (p.Data.Disconnected || p.Data.IsDead || p.PlayerId == Player.Id || p.inVent) continue;

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
            bool killingInnocent = !CurrentTarget.IsImpostor;
            Player.MurderPlayer(CurrentTarget);
            if (killingInnocent) Player.MurderPlayer(Player);
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