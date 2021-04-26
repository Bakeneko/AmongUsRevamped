using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUsRevamped.Events;
using AmongUsRevamped.UI;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Options
{
    [HarmonyPatch]
    public partial class CustomOption
    {
        public static CustomExporterOption exporter;
        public static CustomImporterOption importer;
        private static Scroller Scroller;
        private static Vector3 LastScrollPosition;

        public static void CreateExporter()
        {
            if (exporter != null) return;

            exporter = new CustomExporterOption("exporter", "Export Custom Settings");
        }

        public static void CreateImporter()
        {
            if (importer != null) return;

            importer = new CustomImporterOption("importer", "Import Custom Settings");
        }

        public static void StartImportExport()
        {
            if (exporter != null)
            {
                exporter.MenuVisible = false;
                exporter.SlotButtons?.Clear();
            }

            if (importer != null)
            {
                importer.MenuVisible = false;
                importer.SlotButtons?.Clear();
            }
        }

        public static void EndImportExport()
        {
            if (exporter != null)
            {
                if (exporter.SlotButtons != null)
                {
                    foreach (var option in exporter.SlotButtons)
                    {
                        Options.Remove(option);
                    }
                    exporter.SlotButtons.Clear();
                }
                exporter.PreviousOptions?.Clear();
                exporter.MenuVisible = true;
            }

            if (importer != null)
            {
                if (importer.SlotButtons != null)
                {
                    foreach (var option in importer.SlotButtons)
                    {
                        Options.Remove(option);
                    }
                    importer.SlotButtons.Clear();
                }
                importer.PreviousOptions?.Clear();
                importer.MenuVisible = true;
            }
        }

        private static List<OptionBehaviour> GetGameOptions(GameOptionsMenu __instance) 
        {

            List<OptionBehaviour> options = new();

            ToggleOption toggleOption = Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();
            NumberOption numberOption = Object.FindObjectsOfType<NumberOption>().FirstOrDefault();
            StringOption stringOption = Object.FindObjectsOfType<StringOption>().FirstOrDefault();

            EndImportExport();

            if (exporter != null)
            {
                if (AmongUsClient.Instance?.AmHost == true && toggleOption != null)
                {
                    ToggleOption toggle = Object.Instantiate(toggleOption, toggleOption.transform.parent);
                    if (!exporter.OnGameObjectCreated(toggle))
                    {
                        toggle?.gameObject?.Destroy();
                    }
                    else
                    {
                        options.Add(exporter.GameObject);
                    }
                }
            }

            if (importer != null)
            {
                if (AmongUsClient.Instance?.AmHost == true && toggleOption != null)
                {
                    ToggleOption toggle = Object.Instantiate(toggleOption, toggleOption.transform.parent);
                    if (!importer.OnGameObjectCreated(toggle))
                    {
                        toggle?.gameObject?.Destroy();
                    }
                    else
                    {
                        options.Add(importer.GameObject);
                    }
                }
            }

            // Default options
            foreach (var option in __instance.Children)
            {
                options.Add(option);
            }

            foreach (CustomOption option in Options)
            {
                if (option is CustomExporterOption || option is CustomImporterOption) continue;

                if (option.GameObject)
                {
                    option.GameObject.gameObject.SetActive(option.MenuVisible);
                    options.Add(option.GameObject);
                    continue;
                }

                if (option.Type == CustomOptionType.Button)
                {
                    OptionBehaviour obj;
                    if (AmongUsClient.Instance?.AmHost == true)
                    {
                        if (toggleOption == null) continue;
                        obj = Object.Instantiate(toggleOption, toggleOption.transform.parent);
                    } else {
                        if (stringOption == null) continue;
                        obj = Object.Instantiate(stringOption, stringOption.transform.parent);
                    }

                    if (!option.OnGameObjectCreated(obj))
                    {
                        obj?.gameObject?.Destroy();
                        continue;
                    }

                    options.Add(obj);
                }
                else if (option.Type == CustomOptionType.Toggle)
                {
                    OptionBehaviour obj;
                    if (AmongUsClient.Instance?.AmHost == true)
                    {
                        if (toggleOption == null) continue;
                        obj = Object.Instantiate(toggleOption, toggleOption.transform.parent);
                    }
                    else
                    {
                        if (stringOption == null) continue;
                        obj = Object.Instantiate(stringOption, stringOption.transform.parent);
                    }

                    if (!option.OnGameObjectCreated(obj))
                    {
                        obj?.gameObject?.Destroy();
                        continue;
                    }

                    options.Add(obj);
                }
                else if (option.Type == CustomOptionType.Number)
                {
                    if (numberOption == null) continue;

                    NumberOption number = Object.Instantiate(numberOption, numberOption.transform.parent);

                    if (!option.OnGameObjectCreated(number))
                    {
                        number?.gameObject?.Destroy();
                        continue;
                    }

                    options.Add(number);
                }
                else if (option.Type == CustomOptionType.String)
                {
                    if (stringOption == null) continue;

                    StringOption str = Object.Instantiate(stringOption, stringOption.transform.parent);

                    if (!option.OnGameObjectCreated(str))
                    {
                        str?.gameObject?.Destroy();
                        continue;
                    }

                    options.Add(str);
                }

                if (!option.GameObject) continue;

                option.GameObject.gameObject.SetActive(option.MenuVisible);
            }

            return options;
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        [HarmonyPostfix]
        private static void GameOptionsMenuStart(GameOptionsMenu __instance)
        {
            List<OptionBehaviour> options = GetGameOptions(__instance);

            float y = __instance.GetComponentsInChildren<OptionBehaviour>().Count > 0 ?
                __instance.GetComponentsInChildren<OptionBehaviour>().Max(option => option.transform.localPosition.y) : 0;

            // Calculate position for visible options
            int i = 0;
            foreach (var option in __instance.Children)
            {
                if (!option.gameObject.active) continue;

                option.transform.localPosition = new Vector3(option.transform.localPosition.x, y - i++ * 0.5f,
                    option.transform.localPosition.z);
            }

            __instance.Children = options.ToArray();
        }
        
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        [HarmonyPostfix]
        private static void GameOptionsMenuUpdate(GameOptionsMenu __instance)
        {
            float y = __instance.GetComponentsInChildren<OptionBehaviour>().Count > 0 ?
                __instance.GetComponentsInChildren<OptionBehaviour>().Max(option => option.transform.localPosition.y) : 0;

            foreach (CustomOption option in Options)
            {
                if (!option?.GameObject) continue;
                option?.GameObject?.gameObject?.SetActive(option?.MenuVisible ?? false);
            }

            // Calculate position for visible options
            int i = 0;
            foreach (var option in __instance.Children)
            {
                if (option?.gameObject?.active != true) continue;

                option.transform.localPosition = new Vector3(option.transform.localPosition.x, y - i++ * 0.5f,
                    option.transform.localPosition.z);
            }

            __instance.GetComponentInParent<Scroller>().YBounds.max = (__instance.Children.Length - 7) * 0.5F + 0.13F;
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.SetRecommendations))]
        [HarmonyPostfix]
        private static void GameOptionsDataSetRecommendations()
        {
            foreach (CustomOption option in Options)
            {
                option.SetToDefault();
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.ToHudString))]
        [HarmonyPostfix]
        private static void GameOptionsDataToHudString(ref string __result)
        {
            int firstNewline = __result.IndexOf('\n');

            StringBuilder sb = new(ClearDefaultHudText ? __result.Substring(0, firstNewline + 1) : __result);

            foreach (CustomOption option in Options)
            {
                if (!option.HudVisible) continue;

                string prefix = "";
                if (option is CustomHeaderOption) prefix += "\n";
                if (option.Indent) prefix += "    ";
                
                sb.AppendLine(prefix + option.ToString());
            }

            __result = sb.ToString();

            string insert = ClearDefaultHudText ? " - Press Tab to expand" : " - Press Tab to collapse";

            if (Scrollable && (HudManager.Instance?.GameSettings?.renderedHeight).GetValueOrDefault() + 0.02F > HudPosition.Height)
            {
                insert += " (Scroll for more):";
            }

            __result = __result.Insert(firstNewline - 1, insert);

            // Remove last newline (for the scroller to not overscroll one line)
            __result = __result[0..^1];
        }


        /// <summary>
        /// Component used to expand or collapse game options
        /// </summary>
        [RegisterInIl2Cpp]
        private class LobbyGameOptionsKeyboardShim : MonoBehaviour
        {
            public LobbyGameOptionsKeyboardShim(IntPtr ptr) : base(ptr)
            {
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ClearDefaultHudText = !ClearDefaultHudText;
                }
            }
        }

        /// <summary>
        /// Attach <see cref="LobbyGameOptionsKeyboardShim"/> to <see cref="LobbyBehaviour"/> to expand or collapse game options 
        /// </summary>
        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        private static void LobbyBehaviourStartPatch (LobbyBehaviour __instance)
        {
            _ = __instance.gameObject.GetComponent<LobbyGameOptionsKeyboardShim>() ??
                __instance.gameObject.AddComponent<LobbyGameOptionsKeyboardShim>();
        }

        private static bool OnEnable(OptionBehaviour opt)
        {
            CustomOption customOption = Options.FirstOrDefault(option => option.GameObject == opt);

            if (customOption == null) return true;

            if (!customOption.OnGameObjectCreated(opt)) opt?.gameObject?.Destroy();

            return false;
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        private static class ToggleOptionOnEnablePatch
        {
            private static bool Prefix(ToggleOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        private static class NumberOptionOnEnablePatch
        {
            private static bool Prefix(NumberOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
        private static class StringOptionOnEnablePatch
        {
            private static bool Prefix(StringOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        private static bool OnFixedUpdate(OptionBehaviour opt)
        {
            return !Options.Any(option => option.GameObject == opt);
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.FixedUpdate))]
        private static class NumberOptionFixedUpdatePatch
        {
            private static bool Prefix(NumberOption __instance)
            {
                return OnFixedUpdate(__instance);
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
        private static class StringOptionFixedUpdatePatch
        {
            private static bool Prefix(StringOption __instance)
            {
                return OnFixedUpdate(__instance);
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
        private class ToggleButtonPatch
        {
            public static bool Prefix(ToggleOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameObject == __instance);

                if (option is IToggleOption toggle) toggle.Toggle();
                else if (option is IButtonOption button) button.Click();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        private class NumberOptionPatchIncrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameObject == __instance);

                if (option is INumberOption number) number.Increase();

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        private class NumberOptionPatchDecrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameObject == __instance);

                if (option is INumberOption number) number.Decrease();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        private class StringOptionPatchIncrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameObject == __instance);

                if (option is IStringOption str) str.Increase();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        private class StringOptionPatchDecrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameObject == __instance);

                if (option is IStringOption str) str.Decrease();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        private class PlayerControlPatch
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance?.AmHost != true || PlayerControl.AllPlayerControls.Count < 2 || !PlayerControl.LocalPlayer) return;

                foreach (CustomOption option in Options) if (option.SendRpc) Rpc.Instance.Send(option, true);
            }
        }

        static CustomOption()
        {
            HudEvents.OnHudUpdate += UpdateScroller;
        }

        private static void UpdateScroller(object sender, EventArgs e)
        {
            HudManager hudManager = (HudManager)sender;

            if (hudManager?.GameSettings?.transform == null) return;
            
            hudManager.GameSettings.fontSize = HudTextFontSize;

            const float XOffset = 0.066666F, YOffset = 0.1F;

            // Scroller disabled
            if (!Scrollable)
            {
                // Remove scroller if disabled late
                if (Scroller != null)
                {
                    hudManager.GameSettings.transform.SetParent(Scroller.transform.parent);
                    hudManager.GameSettings.transform.localPosition = new HudPosition(XOffset, YOffset, HudAlignment.TopLeft);

                    Scroller?.Destroy();
                }

                return;
            }

            CreateScroller(hudManager);

            // Update visibility
            Scroller.gameObject.SetActive(hudManager.GameSettings.gameObject.activeSelf);

            if (!Scroller.gameObject.active) return;

            // Scroll range
            Scroller.YBounds = new FloatRange(HudPosition.TopLeft.y, Mathf.Max(HudPosition.TopLeft.y, hudManager.GameSettings.renderedHeight - HudPosition.TopLeft.y + 0.02F));

            float x = HudPosition.TopLeft.x + XOffset;
            Scroller.XBounds = new FloatRange(x, x);

            Vector3 pos = hudManager.GameSettings.transform.localPosition;
            if (pos.x != x) // Resolution updated
            {
                pos.x = x;

                hudManager.GameSettings.transform.localPosition = pos;
            }

            // Prevent scrolling when the player is interacting with a menu
            if (PlayerControl.LocalPlayer?.CanMove != true)
            {
                hudManager.GameSettings.transform.localPosition = LastScrollPosition.x == x ? LastScrollPosition : (LastScrollPosition = HudPosition.TopLeft + new Vector2(XOffset, 0));

                return;
            }

            // Don't save position if not ready
            if (hudManager.GameSettings.transform.localPosition.y < HudPosition.TopLeft.y) return;

            LastScrollPosition = hudManager.GameSettings.transform.localPosition;
        }

        private static void CreateScroller(HudManager hudManager)
        {
            if (Scroller != null) return;

            Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
            Scroller.transform.SetParent(hudManager.GameSettings.transform.parent);
            Scroller.gameObject.layer = 5;

            Scroller.transform.localScale = Vector3.one;
            Scroller.allowX = false;
            Scroller.allowY = true;
            Scroller.active = true;
            Scroller.velocity = new Vector2(0, 0);
            Scroller.ScrollerYRange = new FloatRange(0, 0);
            Scroller.enabled = true;

            Scroller.Inner = hudManager.GameSettings.transform;
            hudManager.GameSettings.transform.SetParent(Scroller.transform);
        }
    }
}
