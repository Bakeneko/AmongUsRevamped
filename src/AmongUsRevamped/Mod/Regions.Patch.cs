using System;
using System.Linq;
using AmongUsRevamped.Options;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public static class RegionsPatch
    {
        private static IRegionInfo[] defaultRegions;
        private static CustomStringNames customRegionName;

        private static TextBoxTMP serverAddressText;
        private static TextBoxTMP serverPortText;

        public static void LoadRegions()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            // Get default regions on first load
            if (defaultRegions == null)
            {
                defaultRegions = ServerManager.DefaultRegions;
            }
            IRegionInfo[] regions = defaultRegions;

            if (customRegionName == null)
            {
                customRegionName = CustomStringNames.Register("Custom");
            }

            // Generate custom region
            var CustomRegion = new DnsRegionInfo(
                CustomSettings.ServerAddress.Value,
                customRegionName.Value,
                customRegionName,
                null,
                CustomSettings.ServerPort.Value
            );

            // Inject to regions
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
        public static void RegionMenuOpenPatch(RegionMenu __instance)
        {
            var gameIdText = DestroyableSingleton<JoinGameButton>.Instance.GameIdText;

            // Instantiate server address input
            if (serverAddressText == null || serverAddressText.gameObject == null)
            {
                serverAddressText = Object.Instantiate(gameIdText, __instance.transform);
                serverAddressText.gameObject.name = "ServerAddressText";
                Object.DestroyImmediate(serverAddressText.transform.FindChild("arrowEnter").gameObject);

                serverAddressText.transform.localPosition = new Vector3(0, -1f, -100f);
                serverAddressText.characterLimit = 30;
                serverAddressText.AllowSymbols = true;
                serverAddressText.ForceUppercase = false;
                // Delay a bit before setting text
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
                {
                    serverAddressText.outputText.SetText(CustomSettings.ServerAddress.Value);
                    serverAddressText.SetText(CustomSettings.ServerAddress.Value);
                })));
                serverAddressText.ClearOnFocus = false;
                serverAddressText.OnEnter = serverAddressText.OnChange = new Button.ButtonClickedEvent();
                serverAddressText.OnFocusLost = new Button.ButtonClickedEvent();
                serverAddressText.OnChange.AddListener((UnityAction)delegate
                {
                    CustomSettings.ServerAddress.Value = serverAddressText.text;
                });
                serverAddressText.OnFocusLost.AddListener((UnityAction)delegate
                {
                    LoadRegions();
                    __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
                });
            }
            
            // Instantiate server port input
            if (serverPortText == null || serverPortText.gameObject == null)
            {
                serverPortText = Object.Instantiate(gameIdText, __instance.transform);
                serverPortText.gameObject.name = "ServerPortText";
                Object.DestroyImmediate(serverPortText.transform.FindChild("arrowEnter").gameObject);

                serverPortText.transform.localPosition = new Vector3(0, -1.75f, -100f);
                serverPortText.characterLimit = 5;
                // Delay a bit before setting text
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
                {
                    serverPortText.outputText.SetText(CustomSettings.ServerPort.Value.ToString());
                    serverPortText.SetText(CustomSettings.ServerPort.Value.ToString());
                })));
                serverPortText.ClearOnFocus = false;
                serverPortText.OnEnter = serverPortText.OnChange = new Button.ButtonClickedEvent();
                serverPortText.OnFocusLost = new Button.ButtonClickedEvent();
                serverPortText.OnChange.AddListener((UnityAction)delegate
                {
                    if (ushort.TryParse(serverPortText.text, out ushort port))
                    {
                        CustomSettings.ServerPort.Value = port;
                        serverPortText.outputText.color = Color.white;
                    }
                    else
                    {
                        serverPortText.outputText.color = Color.red;
                    }
                });
                serverPortText.OnFocusLost.AddListener((UnityAction)delegate
                {
                    LoadRegions();
                    __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
                });
            }
        }

        /// <summary>
        /// Fix 2021.4.1X region text
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchMaker), nameof(MatchMaker.Start))]
        private static void MatchMakerStartPatch(MatchMaker __instance)
        {
            var normalMenu = __instance.transform.parent;
            if (normalMenu == null) return;

            for (int i = 0; i < normalMenu.transform.childCount; i++)
            {
                var child = normalMenu.transform.GetChild(i);
                if (child.name.Equals("RegionButton"))
                {
                    var regionText = child.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (regionText == null) return;

                    var region = DestroyableSingleton<ServerManager>.Instance.CurrentRegion;
                    regionText.text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(region.TranslateName, region.Name, Array.Empty<Il2CppSystem.Object>());
                    return;
                }
            }
        }
    }
}
