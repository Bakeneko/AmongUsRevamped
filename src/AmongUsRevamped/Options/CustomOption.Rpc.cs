using System.Linq;
using AmongUsRevamped.Utils;
using AmongUsRevamped.Mod;
using BepInEx.IL2CPP;
using Hazel;
using Reactor;
using Reactor.Networking;

namespace AmongUsRevamped.Options
{
    public partial class CustomOption
    {

        [RegisterCustomRpc((uint)CustomRpcCalls.SettingsSync)]
        private protected class Rpc : PlayerCustomRpc<BasePlugin, (byte[], CustomOptionType, object)>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

            public Rpc(BasePlugin plugin, uint id) : base(plugin, id)
            {
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

            public override void Write(MessageWriter writer, (byte[], CustomOptionType, object) option)
            {
                writer.Write(option.Item1); // Hash
                writer.Write((byte)option.Item2); // Type
                // Value
                if (option.Item2 == CustomOptionType.Toggle) writer.Write((bool)option.Item3);
                else if (option.Item2 == CustomOptionType.Number) writer.Write((float)option.Item3);
                else if (option.Item2 == CustomOptionType.String) writer.Write((int)option.Item3);
            }

            public override (byte[], CustomOptionType, object) Read(MessageReader reader)
            {
                byte[] hash = reader.ReadBytes(HashUtils.Length);
                CustomOptionType type = (CustomOptionType)reader.ReadByte();
                object value = null;
                if (type == CustomOptionType.Toggle) value = reader.ReadBoolean();
                else if (type == CustomOptionType.Number) value = reader.ReadSingle();
                else if (type == CustomOptionType.String) value = reader.ReadInt32();

                return (hash, type, value);
            }

            public override void Handle(PlayerControl sender, (byte[], CustomOptionType, object) option)
            {
                if (sender?.Data == null) return;

                // Retrieve option
                byte[] hash = option.Item1;
                CustomOptionType type = option.Item2;
                CustomOption customOption = Options.FirstOrDefault(o => o.Type == type && o.Hash.SequenceEqual(hash));

                if (customOption == null)
                {
                    AmongUsRevamped.Logger.LogWarning($"Received option that could not be found, hash: \"{string.Join("", hash.Select(b => $"{b:X2}"))}\", type: {type}.");
                    return;
                }

                object value = option.Item3;

                if (Debug) AmongUsRevamped.Logger.LogDebug($"\"{customOption.Id}\" type: {type}, value: {value}, current value: {customOption.Value}");

                customOption.SetValue(value, true);

                if (Debug) AmongUsRevamped.Logger.LogDebug($"\"{customOption.Id}\", set value: {customOption.Value}");
            }
        }

        public static implicit operator (byte[] Hash, CustomOptionType Type, object Value)(CustomOption option)
        {
            return (option.Hash, option.Type, option.GetValue<object>());
        }
    }
}
