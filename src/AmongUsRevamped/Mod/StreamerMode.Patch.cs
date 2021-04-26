﻿using System.Collections.Generic;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Options;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class StreamerModePatch
    {
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static class OptionsMenuBehaviourStartPatch
        {
            private static ToggleButtonBehaviour streamerModeButton;

            private static void UpdateStreamerModeButton()
            {
                if (streamerModeButton == null || streamerModeButton.gameObject == null) return;

                bool active = CustomSettings.StreamerMode.Value;
                Color color = active ? ColorPalette.Color.SettingGreen : Color.white;
                streamerModeButton.Background.color = color;
                streamerModeButton.Text.text = $"Streamer Mode: {(active ? "On" : "Off")}";
                if (streamerModeButton.Rollover) streamerModeButton.Rollover.ChangeOutColor(color);
            }

            public static void Postfix(OptionsMenuBehaviour __instance)
            {
                if ((streamerModeButton == null || streamerModeButton.gameObject == null) && __instance.CensorChatButton != null)
                {
                    streamerModeButton = Object.Instantiate(__instance.CensorChatButton, __instance.CensorChatButton.transform.parent);
                    streamerModeButton.transform.localPosition += Vector3.down * 0.25f;
                    __instance.CensorChatButton.transform.localPosition += Vector3.up * 0.25f;
                    PassiveButton button = streamerModeButton.GetComponent<PassiveButton>();
                    button.OnClick = new Button.ButtonClickedEvent();
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
                    {
                        CustomSettings.StreamerMode.Value = !CustomSettings.StreamerMode.Value;
                        UpdateStreamerModeButton();
                    });
                    UpdateStreamerModeButton();
                }
            }
        }

        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
        public static class TextBoxTMPStreamerModeFilterPatch
        {
            private static readonly HashSet<string> filter = new()
            {
                "GameIdText",
                "ServerAddressText",
                "ServerPortText"
            };
            private static void Postfix(TextBoxTMP __instance)
            {
                if(CustomSettings.StreamerMode.Value && filter.Contains(__instance.name))
                    __instance.outputText.text = new string('*', __instance.text.Length);
            }
        }
    }
}
