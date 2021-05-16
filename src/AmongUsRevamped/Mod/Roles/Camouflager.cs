using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using Hazel;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Camouflager : Impostor
    {
        public CooldownButton CamouflageButton = null;
        public AudioClip CamouflageSound = null;

        public bool CamouflageActive => CamouflageTime > 0f;
        public float CamouflageTime = 0f;

        public Camouflager(Player player) : base(player)
        {
            Name = "Camouflager";
            RoleType = RoleType.Camouflager;
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => "Camouflage and turn everyone grey";
            TaskDescription = () => Color.ToColorTag($"{Name}: Camouflage for some sneaky kills");
            ExileDescription = () => $"{Player.Name} was The {Name}";
        }

        protected override void Init()
        {
            base.Init();
            if (Player.IsCurrentPlayer)
            {
                CamouflageButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_camouflager_camouflage.png", new HudPosition(GameButton.ButtonSize, GameButton.ButtonSize, HudAlignment.BottomRight),
                    Options.Values.CamouflagerCamouflageCooldown, Options.Values.CamouflagerCamouflageDuration, 10f)
                {
                    HotKey = KeyCode.F,
                    Clickable = false,
                    Visible = true
                };
                CamouflageButton.EffectStarted += OnCamouflageStarted;
                CamouflageButton.EffectEnded += OnCamouflageEnded;
                CamouflageButton.ApplyCooldown(CamouflageButton.InitialCooldownDuration);
            }

            CamouflageSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_camouflager_camouflage.wav");
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
            CamouflageUpdate();
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);

            if (CamouflageButton != null)
            {
                CamouflageButton.Visible = !Player.Dead;
                CamouflageButton.Clickable = Player.CanMove;
            }
        }
        
        public void OnCamouflageStarted(object sender, EventArgs e)
        {
            CamouflageRpc.Instance.Send(true, true);
            Camouflage();
        }

        public void OnCamouflageEnded(object sender, EventArgs e)
        {
            CamouflageRpc.Instance.Send(false, true);
            UnCamouflage();
        }

        public void Camouflage()
        {
            try
            {
                SoundManager.Instance.PlaySound(CamouflageSound, false, 1.0f);
                CamouflageTime = Options.Values.CamouflagerCamouflageDuration;
            }
            catch (Exception) { }    
        }

        public void CamouflageUpdate()
        {
            try
            {
                if (!CamouflageActive) return;

                CamouflageTime -= Time.deltaTime;

                if (!CamouflageActive || Player.Dead || Player.Disconnected)
                {
                    UnCamouflage();
                }
            }
            catch (Exception) { }
        }

        public void UnCamouflage()
        {
            try
            {
                SoundManager.Instance.PlaySound(CamouflageSound, false, 1.0f);
                CamouflageTime = 0f;
                if (CamouflageButton != null) CamouflageButton.EndEffect(false, true);
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
                    UnCamouflage();

                    CamouflageButton?.Dispose();
                    CamouflageButton = null;

                    CamouflageSound?.Destroy();
                    CamouflageSound = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.Camouflage)]
        private protected class CamouflageRpc : PlayerCustomRpc<bool>
        {
            public static CamouflageRpc Instance { get { return Rpc<CamouflageRpc>.Instance; } }

            public CamouflageRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, bool status)
            {
                writer.Write(status); // Camouflage started/ended
            }

            public override bool Read(MessageReader reader)
            {
                return reader.ReadBoolean();
            }

            public override void Handle(PlayerControl sender, bool status)
            {
                var camouflager = GetPlayerRole<Camouflager>(sender.PlayerId);
                if (status) camouflager?.Camouflage(); else camouflager?.UnCamouflage();
            }
        }
    }
}
