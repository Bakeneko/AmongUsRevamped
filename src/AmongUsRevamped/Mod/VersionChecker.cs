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
        public static Dictionary<byte, Tuple<byte, byte, byte>> playerVersions = new();
        private static bool VersionSent = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        [HarmonyPostfix]
        public static void GameStartManagerStartPatch()
        {
            // Trigger version refresh
            VersionSent = false;
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        [HarmonyPostfix]
        public static void GameStartManagerUpdatePatch(GameStartManager __instance)
        {
            // Send version as soon as possible
            if (PlayerControl.LocalPlayer != null && !VersionSent)
            {
                VersionSent = true;
                Rpc.Instance.SendTo(AmongUsClient.Instance.HostId,
                    new Tuple<byte, byte, byte>(AmongUsRevamped.Major, AmongUsRevamped.Minor, AmongUsRevamped.Patch));
            }

            // Host update infos with handshakes
            if (AmongUsClient.Instance.AmHost)
            {
                bool blockStart = false;
                string message = "";
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                {
                    // Skip dummies
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled) continue;
                    var player = client.Character.Data;

                    if (!playerVersions.TryGetValue(player.PlayerId, out Tuple<byte, byte, byte> version))
                    {
                        blockStart = true;
                        message += Color.Error.ToColorTag($"{player.PlayerName} has no version of Revamped\n");
                    }
                    else if (version.Item1 != AmongUsRevamped.Major || version.Item2 != AmongUsRevamped.Minor || version.Item3 != AmongUsRevamped.Patch)
                    {
                        blockStart = true;
                        message += Color.Error.ToColorTag($"{player.PlayerName} has a different version v{version.Item1}.{version.Item2}.{version.Item3} of Revamped\n");
                    }
                }
                if (blockStart)
                {
                    __instance.StartButton.color = Palette.DisabledClear;
                    __instance.GameStartText.text = message;
                    __instance.GameStartText.fontSize = 3.5f;
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                }
                else
                {
                    __instance.GameStartText.fontSize = 5.0f;
                    __instance.StartButton.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        [HarmonyPrefix]
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

                    if (!playerVersions.TryGetValue(client.Character.Data.PlayerId, out Tuple<byte, byte, byte> version))
                    {
                        canBegin = false;
                    }
                    else if (version.Item1 != AmongUsRevamped.Major || version.Item2 != AmongUsRevamped.Minor || version.Item3 != AmongUsRevamped.Patch)
                    {
                        canBegin = false;
                    }
                }
            }

            return canBegin;
        }

        [RegisterCustomRpc((uint)CustomRpcCalls.VersionCheck)]
        private protected class Rpc : PlayerCustomRpc<BasePlugin, Tuple<byte, byte, byte>>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

            public Rpc(BasePlugin plugin, uint id) : base(plugin, id)
            {
            }

            public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
            public override void Write(MessageWriter writer, Tuple<byte, byte, byte> handshake)
            {
                writer.Write(handshake.Item1);
                writer.Write(handshake.Item2);
                writer.Write(handshake.Item3);
            }

            public override Tuple<byte, byte, byte> Read(MessageReader reader)
            {
                return new Tuple<byte, byte, byte>(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            }

            public override void Handle(PlayerControl sender, Tuple<byte, byte, byte> handshake)
            {
                if (sender?.Data == null) return;

                playerVersions[sender.Data.PlayerId] = handshake;
            }
        }
    }
}
