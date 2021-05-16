using System;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using Hazel;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Engineer : Crewmate
    {
        public CooldownButton RepairButton = null;
        public AudioClip RepairSound = null;

        public int Repairs = 0;

        public readonly int RepairsOption = Options.Values.EngineerRepairs;

        public Engineer(Player player) : base(player, RoleType.Engineer)
        {
            Name = "Engineer";
            Color = ColorPalette.Color.RoleEngineer;
            IntroDescription = () => "Maintain important systems on the ship";
            TaskDescription = () => Color.ToColorTag($"{Name}: Vent and fix a sabotage from anywhere!");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Init();
        }

        protected void Init()
        {
            if (Player.IsCurrentPlayer)
            {
                RepairButton = new CooldownButton("AmongUsRevamped.Resources.Sprites.button_engineer_repair.png", new HudPosition(GameButton.ButtonSize, 0f, HudAlignment.BottomRight), 10f, 0f, 10f)
                {
                    HotKey = KeyCode.Q,
                    Clickable = false,
                    Visible = true
                };
                RepairButton.Clicked += OnRepairButtonClicked;
                RepairButton.ApplyCooldown(RepairButton.InitialCooldownDuration);
                RepairSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.effect_engineer_repair.wav");
            }

            Repairs = RepairsOption switch
            {
                0 => 1, // 1 per game
                1 => 2, // 2 per game
                2 => 3, // 3 per game
                3 => 1, // 1 per round
                _ => 1 // Should never happen
            };
        }

        protected internal override bool CanUseVent(Vent vent)
        {
            return true;
        }

        public override void HudUpdate(HudManager hudManager)
        {
            base.HudUpdate(hudManager);
            UpdateVentOutlines();
        }

        public override void CurrentPlayerHudUpdate(HudManager hudManager)
        {
            if (RepairButton != null)
            {
                RepairButton.Visible = !Player.Dead;
                RepairButton.Clickable = Repairs > 0 && Player.CanMove && SearchForRepair();
            }
        }

        public override void OnExileEnd(ExileController exileController)
        {
            if (RepairsOption == 3) // 1 per round
            {
                Repairs = 1;
            }
        }

        protected override void UpdateVentOutlines()
        {
            if (ShipStatus.Instance?.AllVents == null) return;

            if (Player.IsCurrentPlayer)
            {
                try
                {
                    foreach (Vent vent in ShipStatus.Instance.AllVents)
                    {
                        vent.myRend.SetOutline(Player.CanUseVent(vent) ? Color : null);
                    }
                }
                catch { }
            }
            else if (Player.CurrentPlayer?.Role?.Faction == Faction.Impostors)
            {
                var currentPlayer = Player.CurrentPlayer;
                var color = currentPlayer.Role.Color;
                var inVent = GetRoles<Engineer>(RoleType.Engineer).Any(r => r.Player.Control.inVent);
                try
                {
                    foreach (Vent vent in ShipStatus.Instance.AllVents)
                    {
                        if (vent.myRend?.material == null) continue;
                        vent.myRend.SetOutline(inVent ? Color : currentPlayer.CanUseVent(vent) ? color : null);
                    }
                }
                catch { }
            }
        }

        public bool SearchForRepair()
        {
            if (!ShipStatus.Instance) return false;

            var systems = ShipStatus.Instance.Systems;

            var lights = systems.ContainsKey(SystemTypes.Electrical) ? systems[SystemTypes.Electrical].TryCast<SwitchSystem>() : null;
            if (lights?.IsActive == true) return true;

            var lifeSupp = systems.ContainsKey(SystemTypes.LifeSupp) ? systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>() : null;
            if (lifeSupp?.IsActive == true) return true;

            var reactor = systems.ContainsKey(SystemTypes.Reactor) ? systems[SystemTypes.Reactor].TryCast<ICriticalSabotage>() : null;
            if (reactor?.IsActive == true) return true;

            var laboratory = systems.ContainsKey(SystemTypes.Laboratory) ? systems[SystemTypes.Laboratory].TryCast<ICriticalSabotage>() : null;
            if (laboratory?.IsActive == true) return true;

            var commsSys = systems.ContainsKey(SystemTypes.Comms) ? systems[SystemTypes.Comms] : null;
            if (PlayerControl.GameOptions.MapId == 1)
            {
                var comms = commsSys.TryCast<HqHudSystemType>();
                if (comms?.IsActive == true) return true;
            }
            else
            {
                var comms = commsSys.TryCast<HudOverrideSystemType>();
                if (comms?.IsActive == true) return true;
            }

            return false;
        }

        public bool Repair()
        {
            bool repaired = false;

            if (!ShipStatus.Instance) return repaired;

            var systems = ShipStatus.Instance.Systems;

            var lights = systems.ContainsKey(SystemTypes.Electrical) ? systems[SystemTypes.Electrical].TryCast<SwitchSystem>() : null;
            if (lights?.IsActive == true)
            {
                var xor = lights.ActualSwitches ^ lights.ExpectedSwitches;
                for (int i = 0; i < 5; i++)
                {
                    // Activate light switch if need be
                    if ((xor & (int)Mathf.Pow(2, i)) > 0) ShipStatus.Instance.RpcRepairSystem(SystemTypes.Electrical, i);
                }
                repaired = true;
            }

            var lifeSupp = systems.ContainsKey(SystemTypes.LifeSupp) ? systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>() : null;
            if (lifeSupp?.IsActive == true)
            {
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                repaired = true;
            }

            var reactor = systems.ContainsKey(SystemTypes.Reactor) ? systems[SystemTypes.Reactor].TryCast<ICriticalSabotage>() : null;
            if (reactor?.IsActive == true)
            {
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                repaired = true;
            }

            var laboratory = systems.ContainsKey(SystemTypes.Laboratory) ? systems[SystemTypes.Laboratory].TryCast<ICriticalSabotage>() : null;
            if (laboratory?.IsActive == true)
            {
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                repaired = true;
            }

            var commsSys = systems.ContainsKey(SystemTypes.Comms) ? systems[SystemTypes.Comms] : null;
            if (PlayerControl.GameOptions.MapId == 1)
            {
                var comms = commsSys.TryCast<HqHudSystemType>();
                if (comms?.IsActive == true)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    repaired = true;
                }
            }
            else
            {
                var comms = commsSys.TryCast<HudOverrideSystemType>();
                if (comms?.IsActive == true)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    repaired = true;
                }
            }

            return repaired;
        }

        protected void OnRepairButtonClicked(object sender, EventArgs e)
        {
            if (Repairs > 0 && Repair())
            {
                SoundManager.Instance.PlaySound(RepairSound, false, 1.0f);
                Repairs--;
                EngineerRepairRpc.Instance.Send((byte)Repairs);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    RepairButton?.Dispose();
                    RepairButton = null;

                    RepairSound?.Destroy();
                    RepairSound = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.EngineerRepair)]
        private protected class EngineerRepairRpc : PlayerCustomRpc<byte>
        {
            public static EngineerRepairRpc Instance { get { return Rpc<EngineerRepairRpc>.Instance; } }

            public EngineerRepairRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, byte repairs)
            {
                writer.Write(repairs); // Remaining repairs
            }

            public override byte Read(MessageReader reader)
            {
                return reader.ReadByte();
            }

            public override void Handle(PlayerControl sender, byte repairs)
            {
                var engineer = GetPlayerRole<Engineer>(sender.PlayerId);
                if (engineer != null) engineer.Repairs = repairs;
            }
        }
    }
}
