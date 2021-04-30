using System;
using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor;
using Reactor.Networking;
using UnityEngine;

using Color = AmongUsRevamped.Colors.ColorPalette.Color;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class VersionChecker
    {
        public static Dictionary<int, Tuple<byte, byte, byte>> playerVersions = new();
        private static bool VersionSent = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public static void GameStartManagerStartPatch()
        {
            // Trigger version refresh
            VersionSent = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static void GameStartManagerUpdatePatch(GameStartManager __instance)
        {
            // Send version as soon as possible
            if (PlayerControl.LocalPlayer != null && !VersionSent)
            {
                VersionSent = true;
                playerVersions[AmongUsClient.Instance.ClientId] = new Tuple<byte, byte, byte>(AmongUsRevamped.Major, AmongUsRevamped.Minor, AmongUsRevamped.Patch);
                Rpc.Instance.Send(new Rpc.VersionHandshake(AmongUsClient.Instance.ClientId, playerVersions[AmongUsClient.Instance.ClientId], true));
            }

            // Retrieve host version
            if (!playerVersions.TryGetValue(AmongUsClient.Instance.HostId, out Tuple<byte, byte, byte> hostVersion)) return;

            // Update infos with handshakes
            bool blockStart = false;
            string message = "";
            foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
            {
                // Skip dummies
                if (client.Character == null) continue;
                var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                if (dummyComponent != null && dummyComponent.enabled) continue;
                var player = client.Character.Data;
                var prefix = PlayerControl.LocalPlayer.PlayerId == player.PlayerId ? "You have" : $"{player?.PlayerName ?? player.PlayerId.ToString()} has";
                
                if (!playerVersions.TryGetValue(client.Id, out Tuple<byte, byte, byte> version))  // Block no mod
                {
                    blockStart = true;
                    message += Color.Error.ToColorTag($"{prefix} no version of Revamped\n");
                }
                else if (version.Item1 != hostVersion.Item1 || version.Item2 != hostVersion.Item2) // Block minor version difference
                {
                    blockStart = true;
                    message += Color.Error.ToColorTag($"{prefix} a different version v{version.Item1}.{version.Item2}.{version.Item3} of Revamped\n");
                } else if (version.Item3 != hostVersion.Item3) { // Warn patch version difference
                    message += Color.Warning.ToColorTag($"{prefix} a different version v{version.Item1}.{version.Item2}.{version.Item3} of Revamped\n");
                }
            }

            // Prevent start
            if (AmongUsClient.Instance.AmHost)
                __instance.StartButton.color = ((!blockStart && __instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);

            __instance.GameStartText.text = message;

            // Display warnings
            if (message.Length > 0)
            {
                __instance.GameStartText.fontSize = 3.5f;
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
            }
            else {
                __instance.GameStartText.fontSize = 5.0f;
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public static bool GameStartManagerBeginGamePatch()
        {
            bool canBegin = true;

            // Host check versions
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
                {
                    // Skip dummies
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled) continue;

                    if (!playerVersions.TryGetValue(client.Id, out Tuple<byte, byte, byte> version)) // Block no mod
                    {
                        canBegin = false;
                    }
                    else if (version.Item1 != AmongUsRevamped.Major || version.Item2 != AmongUsRevamped.Minor) // Block minor version difference
                    {
                        canBegin = false;
                    }
                }
            }

            return canBegin;
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.VersionCheck)]
        private protected class Rpc : PlayerCustomRpc<BasePlugin, Rpc.VersionHandshake>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

            public Rpc(BasePlugin plugin, uint id) : base(plugin, id)
            {
            }

            public readonly struct VersionHandshake
            {
                public readonly int ClientId; 
                public readonly Tuple<byte, byte, byte> Version;
                public readonly bool Ping;

                public VersionHandshake(int clientId, Tuple<byte, byte, byte> version, bool ping)
                {
                    ClientId = clientId;
                    Version = version;
                    Ping = ping;
                }
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
            public override void Write(MessageWriter writer, VersionHandshake handshake)
            {
                writer.WritePacked(handshake.ClientId);
                writer.Write(handshake.Version.Item1);
                writer.Write(handshake.Version.Item2);
                writer.Write(handshake.Version.Item3);
                writer.Write(handshake.Ping);
            }

            public override VersionHandshake Read(MessageReader reader)
            {
                return new VersionHandshake(
                    reader.ReadPackedInt32(),
                    new Tuple<byte, byte, byte>(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    reader.ReadBoolean());
            }

            public override void Handle(PlayerControl sender, VersionHandshake handshake)
            {
                // Received a new version handshake, save it
                if (!playerVersions.TryGetValue(handshake.ClientId, out Tuple<byte, byte, byte> version) || !handshake.Version.Equals(version))
                {
                    playerVersions[handshake.ClientId] = handshake.Version;
                }

                // Send version back
                if (handshake.Ping)
                {
                    Instance.SendTo(handshake.ClientId, new Rpc.VersionHandshake(AmongUsClient.Instance.ClientId,
                        new Tuple<byte, byte, byte>(AmongUsRevamped.Major, AmongUsRevamped.Minor, AmongUsRevamped.Patch),
                        false));
                }
            }
        }
    }
}
