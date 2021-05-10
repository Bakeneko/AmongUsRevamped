using System;
using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using HarmonyLib;
using Hazel;
using UnityEngine;

using ColorPalette = AmongUsRevamped.Colors.ColorPalette;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class VersionChecker
    {
        public static Dictionary<int, Version> playerVersions = new();
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
                playerVersions[AmongUsClient.Instance.ClientId] = (Version)AmongUsRevamped.Version.Clone();
                Rpc.Instance.Send(new Rpc.VersionHandshake(AmongUsClient.Instance.ClientId, playerVersions[AmongUsClient.Instance.ClientId], true));
            }

            // Retrieve host version
            if (!playerVersions.TryGetValue(AmongUsClient.Instance.HostId, out Version hostVersion)) return;

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
                
                if (!playerVersions.TryGetValue(client.Id, out Version version))  // Block no mod
                {
                    blockStart = true;
                    message += ColorPalette.Color.Error.ToColorTag($"{prefix} no version of Revamped\n");
                }
                else
                {
                    int comparison = hostVersion.CompareTo(version);
                    if (comparison != 0)
                    {
                        Color messageColor = ColorPalette.Color.Warning; // Warn patch version difference
                        if (version.Major != hostVersion.Major || version.Minor != hostVersion.Minor) // Block minor version difference
                        {
                            blockStart = true;
                            messageColor = ColorPalette.Color.Error;
                        }

                        if (comparison > 0)
                        {
                            message += messageColor.ToColorTag($"{prefix} an older version v{version.Major}.{version.Minor}.{version.Build} of Revamped\n");
                        }
                        else
                        {
                            message += messageColor.ToColorTag($"{prefix} a newer version v{version.Major}.{version.Minor}.{version.Build} of Revamped\n");
                        }

                    }
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

                    if (!playerVersions.TryGetValue(client.Id, out Version version)) // Block no mod
                    {
                        canBegin = false;
                    }
                    else if (version.Major != AmongUsRevamped.Version.Major || version.Minor != AmongUsRevamped.Version.Minor) // Block minor version difference
                    {
                        canBegin = false;
                    }
                }
            }

            return canBegin;
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.VersionCheck)]
        private protected class Rpc : PlayerCustomRpc<Rpc.VersionHandshake>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

            public Rpc(uint id) : base(id)
            {
            }

            public readonly struct VersionHandshake
            {
                public readonly int ClientId; 
                public readonly Version Version;
                public readonly bool Ping;

                public VersionHandshake(int clientId, Version version, bool ping)
                {
                    ClientId = clientId;
                    Version = version;
                    Ping = ping;
                }
            }

            public override void Write(MessageWriter writer, VersionHandshake handshake)
            {
                writer.WritePacked(handshake.ClientId);
                writer.Write((byte)handshake.Version.Major);
                writer.Write((byte)handshake.Version.Minor);
                writer.Write((byte)handshake.Version.Build);
                writer.Write(handshake.Ping);
            }

            public override VersionHandshake Read(MessageReader reader)
            {
                return new VersionHandshake(
                    reader.ReadPackedInt32(),
                    new Version(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    reader.ReadBoolean());
            }

            public override void Handle(PlayerControl sender, VersionHandshake handshake)
            {
                // Received a new version handshake, save it
                if (!playerVersions.TryGetValue(handshake.ClientId, out Version version) || !handshake.Version.Equals(version))
                {
                    playerVersions[handshake.ClientId] = handshake.Version;
                }

                // Send version back
                if (handshake.Ping)
                {
                    Instance.SendTo(handshake.ClientId, new Rpc.VersionHandshake(AmongUsClient.Instance.ClientId, AmongUsRevamped.Version, false));
                }
            }
        }
    }
}
