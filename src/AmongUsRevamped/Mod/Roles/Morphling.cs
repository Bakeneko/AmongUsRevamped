using System;
using System.ComponentModel;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using Hazel;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Morphling : Impostor
    {
        public CooldownButton MorphButton = null;
        public AudioClip SampleSound = null;
        public AudioClip MorphSound = null;
        public AudioClip UnmorphSound = null;

        public Player SampleTarget { get; set; }
        public Player MorphTarget { get; set; }

        public bool Morphing => MorphTime > 0f;
        public float MorphTime = 0f;

        public Morphling(Player player) : base(player, RoleType.Morphling)
        {
            Name = "Morphling";
            Color = ColorPalette.Color.RoleImpostor;
            IntroDescription = () => "Transform into crewmates";
            TaskDescription = () => Color.ToColorTag($"{Name}: Transform into crewmates");
            ExileDescription = () => $"{Player.Name} was The {Name}";
        }

        protected override void Init()
        {
            base.Init();
            if (Player.IsCurrentPlayer)
            {
                MorphButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_morphling_sample.png", new HudPosition(GameButton.ButtonSize, GameButton.ButtonSize, HudAlignment.BottomRight),
                    Options.Values.MorphlingMorphCooldown, Options.Values.MorphlingMorphDuration, 10f)
                {
                    HotKey = KeyCode.F,
                    Clickable = false,
                    Visible = true
                };
                MorphButton.OnClick += OnMorphButtonClick;
                MorphButton.EffectStarted += OnMorphStarted;
                MorphButton.EffectEnded += OnMorphEnded;
                MorphButton.ApplyCooldown(MorphButton.InitialCooldownDuration);

                SampleSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_morphling_sample.wav");
                MorphSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_morphling_morph.wav");
                UnmorphSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_morphling_unmorph.wav");
            }
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
            MorphUpdate();
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            base.CurrentPlayerHudUpdate(hudManager);

            if (MorphButton != null)
            {
                MorphButton.Visible = !Player.Dead;
                MorphButton.Clickable = Player.CanMove && (SampleTarget != null || CurrentTarget != null);
            }
        }

        public void OnMorphButtonClick(object sender, CancelEventArgs e)
        {
            if (SampleTarget == null && CurrentTarget == null)
            {
                e.Cancel = true;
                return;
            }

            if (SampleTarget == null) 
            {
                SoundManager.Instance.PlaySound(SampleSound, false, 1.0f);
                SampleTarget = CurrentTarget;
                MorphButton.UpdateSprite(AssetUtils.LoadSpriteFromResource("AmongUsRevamped.Resources.Sprites.button_morphling_morph.png"));
                MorphButton.EndEffect(false, false);
                MorphButton.ApplyCooldown(2f);
                e.Cancel = true;
            }
            else
            {
                MorphTarget = SampleTarget;
                SampleTarget = null;
                MorphButton.UpdateSprite(AssetUtils.LoadSpriteFromResource("AmongUsRevamped.Resources.Sprites.button_morphling_sample.png"));
            }
        }

        public void OnMorphStarted(object sender, EventArgs e)
        {
            MorphRpc.Instance.Send(MorphTarget.Id, true);
            Morph();
        }

        public void OnMorphEnded(object sender, EventArgs e)
        {
            MorphRpc.Instance.Send(-1, true);
            Unmorph();
        }

        public void Morph()
        {
            try
            {
                MorphTime = Options.Values.MorphlingMorphDuration;

                if (Player.IsCurrentPlayer) SoundManager.Instance.PlaySound(MorphSound, false, 1.0f);

                var currentPlayer = Player.CurrentPlayer;
                var target = MorphTarget ?? Player;
                var name = target.Data.PlayerName;

                if (Player.IsCurrentPlayer || currentPlayer.Dead || currentPlayer.Role?.Faction == Faction.Impostors)
                {
                    name = Player.Data.PlayerName;
                }

                Disguise = new Disguise(name, target.Data.ColorId, default, target.Data.HatId, target.Data.SkinId, target.Data.PetId, target.Size, target.MoveSpeed);

                ApplyDisguise();
            }
            catch (Exception) { }
        }

        public void MorphUpdate()
        {
            try
            {
                if (!Morphing) return;

                MorphTime -= Time.deltaTime;

                if (!Morphing || Player.Dead || Player.Disconnected)
                {
                    Unmorph();
                    return;
                }

                var currentPlayer = Player.CurrentPlayer;
                var target = MorphTarget ?? Player;

                if (Player.IsCurrentPlayer || currentPlayer.Dead || currentPlayer.Role?.Faction == Faction.Impostors)
                {
                    Disguise = new Disguise(Player.Data.PlayerName, target.Data.ColorId, default, target.Data.HatId, target.Data.SkinId, target.Data.PetId, target.Size, target.MoveSpeed);
                }
                else
                {
                    Disguise = new Disguise(target.Data.PlayerName, target.Data.ColorId, default, target.Data.HatId, target.Data.SkinId, target.Data.PetId, target.Size, target.MoveSpeed);
                }

                ApplyDisguise();
            }
            catch (Exception) { }
        }

        public void Unmorph()
        {
            try
            {
                MorphTime = 0f;

                Disguise = null;
                ApplyDisguise();
                if (Player.IsCurrentPlayer) SoundManager.Instance.PlaySound(UnmorphSound, false, 1.0f);
                if (MorphButton != null) MorphButton.EndEffect(false, true);
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
                    SampleTarget = null;
                    MorphTarget = null;
                    MorphButton?.Dispose();
                    MorphButton = null;

                    SampleSound?.Destroy();
                    SampleSound = null;
                    MorphSound?.Destroy();
                    MorphSound = null;
                    UnmorphSound?.Destroy();
                    UnmorphSound = null;

                    Unmorph();
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.Morph)]
        private protected class MorphRpc : PlayerCustomRpc<int>
        {
            public static MorphRpc Instance { get { return Rpc<MorphRpc>.Instance; } }

            public MorphRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, int target)
            {
                writer.WritePacked(target); // target player id
            }

            public override int Read(MessageReader reader)
            {
                return reader.ReadPackedInt32();
            }

            public override void Handle(PlayerControl sender, int target)
            {
                var morphling = GetPlayerRole<Morphling>(sender.PlayerId);
                if (target == -1)
                {
                    morphling?.Unmorph();
                }
                else
                {
                    var player = Player.GetPlayer(target);
                    morphling.MorphTarget = player;
                    morphling?.Morph();
                }
                
            }
        }
    }
}
