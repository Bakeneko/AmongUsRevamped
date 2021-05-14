using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Spy : Crewmate
    {
        public CooldownButton GadgetButton = null;
        public AudioClip GadgetSound = null;

        public bool GadgetActive => GadgetButton?.IsEffectActive == true;

        private Message WarningMessage;

        public Spy(Player player) : base(player)
        {
            Name = "Spy";
            RoleType = RoleType.Spy;
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => $"Confuse the {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")}";
            TaskDescription = () => Color.ToColorTag($"{Name}: Confuse the Impostors");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Init();
        }

        protected void Init()
        {
            if (Player.IsCurrentPlayer)
            {
                GadgetButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_spy_gadget.png", new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight),
                    Options.Values.SpyGadgetCooldown, Options.Values.SpyGadgetDuration, 0f)
                {
                    HotKey = KeyCode.Q,
                    Clickable = true,
                    Visible = true
                };
                GadgetButton.EffectStarted += OnGadgetStarted;
                GadgetButton.EffectEnded += OnGadgetEnded;
                GadgetButton.ApplyCooldown(GadgetButton.InitialCooldownDuration);

                GadgetSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_spy_gadget.wav");
            }
        }

        public override void OnIntroEnd(IntroCutscene introCutScene)
        {
            base.OnIntroEnd(introCutScene);

            var currentPlayer = Player.CurrentPlayer;
            if (currentPlayer.Role?.Faction == Faction.Impostors && !currentPlayer.Dead && !currentPlayer.Disconnected)
            {
                new Message(6f, $"Beware, there is a {ColorPalette.Color.RoleImpostor.ToColorTag("Spy")} among us !");
            }
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);
            if (Player.Dead)
            {
                if (GadgetButton != null) GadgetButton.Visible = false;
            }
        }

        public void OnGadgetStarted(object sender, EventArgs e)
        {
            SoundManager.Instance.PlaySound(GadgetSound, false, 1.0f);
        }

        public void OnGadgetEnded(object sender, EventArgs e)
        {
            SoundManager.Instance.PlaySound(GadgetSound, false, 1.0f);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    GadgetButton?.Dispose();
                    GadgetButton = null;

                    GadgetSound?.Destroy();
                    GadgetSound = null;

                    WarningMessage?.Dispose();
                    WarningMessage = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }
    }
}
