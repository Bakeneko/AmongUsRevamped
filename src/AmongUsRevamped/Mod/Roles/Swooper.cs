using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using Hazel;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Swooper : Impostor
    {
        public CooldownButton SwoopButton = null;
        public AudioClip SwoopSound = null;
        public AudioClip UnswoopSound = null;

        public bool Swooping => SwoopTime > 0f;
        public float SwoopTime = 0f;

        public Swooper(Player player) : base(player)
        {
            Name = "Swooper";
            RoleType = RoleType.Swooper;
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => "Turn invisible temporarily";
            TaskDescription = () => Color.ToColorTag($"{Name}: Turn invisible for some sneaky kills");
            ExileDescription = () => $"{Player.Name} was The {Name}";
        }

        protected override void Init()
        {
            base.Init();
            if (Player.IsCurrentPlayer)
            {
                SwoopButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_swooper_swoop.png", new HudPosition(GameButton.ButtonSize, GameButton.ButtonSize, HudAlignment.BottomRight),
                    Options.Values.SwooperSwoopCooldown, Options.Values.SwooperSwoopDuration, 10f)
                {
                    HotKey = KeyCode.F,
                    Clickable = false,
                    Visible = true
                };
                SwoopButton.EffectStarted += OnSwoopStarted;
                SwoopButton.EffectEnded += OnSwoopEnded;
                SwoopButton.ApplyCooldown(SwoopButton.InitialCooldownDuration);

                SwoopSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_swooper_swoop.wav");
                UnswoopSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_swooper_unswoop.wav");
            }
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
            SwoopUpdate();
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);

            if (SwoopButton != null)
            {
                SwoopButton.Visible = !Player.Dead;
                SwoopButton.Clickable = Player.CanMove;
            }
        }
        
        public void OnSwoopStarted(object sender, EventArgs e)
        {
            SoundManager.Instance.PlaySound(SwoopSound, false, 1.0f);
            SwoopRpc.Instance.Send(true, true);
            Swoop();
        }

        public void OnSwoopEnded(object sender, EventArgs e)
        {
            SoundManager.Instance.PlaySound(UnswoopSound, false, 1.0f);
            SwoopRpc.Instance.Send(false, true);
            Unswoop();
        }

        public void Swoop()
        {
            try
            {
                SwoopTime = Options.Values.SwooperSwoopDuration;
                var control = Player?.Control;
                if (control == null) return;

                var currentPlayer = Player.CurrentPlayer;
                var color = Color.clear;
                var bodyColor = Color.white;

                if (Player.IsCurrentPlayer || currentPlayer.Dead || currentPlayer.Role?.Faction == Faction.Impostors)
                {
                    bodyColor.a = 0.1f;
                }
                else
                {
                    bodyColor.a = 0f;
                    control.nameText.text = "";
                }

                if (control.myRend != null) control.myRend.color = bodyColor;
                if (control.HatRenderer != null)
                {
                    control.HatRenderer.FrontLayer.color = color;
                    control.HatRenderer.BackLayer.color = color;
                }
                if (control.MyPhysics.Skin != null) control.MyPhysics.Skin.layer.color = color;
                if (control.CurrentPet != null)
                {
                    control.CurrentPet.rend.color = bodyColor;
                    control.CurrentPet.shadowRend.color = bodyColor;
                }
            }
            catch (Exception) { }
        }

        public void SwoopUpdate()
        {
            try
            {
                if (!Swooping) return;

                SwoopTime -= Time.deltaTime;
                
                if (!Swooping) {
                    Unswoop();
                    return;
                }

                var control = Player?.Control;
                if (control == null) return;

                var currentPlayer = Player.CurrentPlayer;
                var color = Color.clear;
                var bodyColor = Color.white;

                if (Player.IsCurrentPlayer || currentPlayer.Dead || currentPlayer.Role?.Faction == Faction.Impostors)
                {
                    bodyColor.a = 0.2f;
                }
                else
                {
                    bodyColor.a = 0f;
                    control.nameText.text = "";
                }

                if (control.myRend != null) control.myRend.color = bodyColor;
                if (control.HatRenderer != null)
                {
                    control.HatRenderer.FrontLayer.color = color;
                    control.HatRenderer.BackLayer.color = color;
                }
                if (control.MyPhysics.Skin != null) control.MyPhysics.Skin.layer.color = color;
                if (control.CurrentPet != null)
                {
                    control.CurrentPet.rend.color = bodyColor;
                    control.CurrentPet.shadowRend.color = bodyColor;
                }
            }
            catch (Exception) { }
        }

        public void Unswoop()
        {
            try
            {
                SwoopTime = 0f;
                var control = Player?.Control;
                if (control == null) return;

                var color = Color.white;

                control.nameText.text = control.Data.PlayerName;

                if (control.myRend != null) control.myRend.color = color;
                if (control.HatRenderer != null)
                {
                    control.HatRenderer.FrontLayer.color = color;
                    control.HatRenderer.BackLayer.color = color;
                }
                if (control.MyPhysics.Skin != null) control.MyPhysics.Skin.layer.color = color;
                if (control.CurrentPet != null)
                {
                    control.CurrentPet.rend.color = color;
                    control.CurrentPet.shadowRend.color = color;
                }
            }
            catch (Exception) { }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    Unswoop();

                    SwoopButton?.Dispose();
                    SwoopButton = null;

                    SwoopSound?.Destroy();
                    SwoopSound = null;
                    UnswoopSound?.Destroy();
                    UnswoopSound = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.Swoop)]
        private protected class SwoopRpc : PlayerCustomRpc<bool>
        {
            public static SwoopRpc Instance { get { return Rpc<SwoopRpc>.Instance; } }

            public SwoopRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, bool status)
            {
                writer.Write(status); // Swoop started/ended
            }

            public override bool Read(MessageReader reader)
            {
                return reader.ReadBoolean();
            }

            public override void Handle(PlayerControl sender, bool status)
            {
                var swooper = GetPlayerRole<Swooper>(sender.PlayerId);
                if (status) swooper?.Swoop(); else swooper?.Unswoop();
            }
        }
    }
}
