using System;
using AmongUsRevamped.Mod.Modifiers;
using AmongUsRevamped.Mod.Roles;
using HarmonyLib;
using Hazel;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public partial class Game
    {
        [RegisterCustomRpc((uint)CustomRpcCalls.RoleAssignation)]
        private protected class RoleAssignationRpc : PlayerCustomRpc<Tuple<byte, byte>>
        {
            public static RoleAssignationRpc Instance { get { return Rpc<RoleAssignationRpc>.Instance; } }

            public RoleAssignationRpc(uint id) : base(id) {}

            public override void Write(MessageWriter writer, Tuple<byte, byte> assignation)
            {
                writer.Write(assignation.Item1); // Player id
                writer.Write(assignation.Item2); // Role id
            }

            public override Tuple<byte, byte> Read(MessageReader reader)
            {
                return new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte());
            }

            public override void Handle(PlayerControl sender, Tuple<byte, byte> assignation)
            {
                OnPlayerRoleAssigned(Player.GetPlayer(assignation.Item1), (RoleType) assignation.Item2);
            }
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.ModifierAssignation)]
        private protected class ModifierAssignationRpc : PlayerCustomRpc<Tuple<byte, byte>>
        {
            public static ModifierAssignationRpc Instance { get { return Rpc<ModifierAssignationRpc>.Instance; } }

            public ModifierAssignationRpc(uint id) : base(id) {}

            public override void Write(MessageWriter writer, Tuple<byte, byte> assignation)
            {
                writer.Write(assignation.Item1); // Player id
                writer.Write(assignation.Item2); // Modifier id
            }

            public override Tuple<byte, byte> Read(MessageReader reader)
            {
                return new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte());
            }

            public override void Handle(PlayerControl sender, Tuple<byte, byte> assignation)
            {
                OnPlayerModifierAssigned(Player.GetPlayer(assignation.Item1), (ModifierType)assignation.Item2);
            }
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.Murder)]
        private protected class MurderRpc : PlayerCustomRpc<Tuple<byte, byte>>
        {
            public static MurderRpc Instance { get { return Rpc<MurderRpc>.Instance; } }

            public MurderRpc(uint id) : base(id) { }

            public override void Write(MessageWriter writer, Tuple<byte, byte> assignation)
            {
                writer.Write(assignation.Item1); // Murderer id
                writer.Write(assignation.Item2); // Victim id
            }

            public override Tuple<byte, byte> Read(MessageReader reader)
            {
                return new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte());
            }

            public override void Handle(PlayerControl sender, Tuple<byte, byte> assignation)
            {
                var killer = Player.GetPlayer(assignation.Item1);
                var victim = Player.GetPlayer(assignation.Item2);
                if (killer != null && victim != null) killer.Control.MurderPlayer(victim.Control);
            }
        }
    }
}
