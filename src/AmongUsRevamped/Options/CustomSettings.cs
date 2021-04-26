using AmongUsRevamped.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using Color = AmongUsRevamped.Colors.ColorPalette.Color;

namespace AmongUsRevamped.Options
{
    [HarmonyPatch]
    public static class CustomSettings
    {
        public static string ConfigSection = "GameSettings";

        public static ConfigEntry<bool> StreamerMode { get; set; }

        /// <summary>
        /// Player body color.
        /// </summary>
        public static ConfigEntry<int> BodyColor { get; set; }

        public static void Load()
        {
            StreamerMode = LoadSetting("streamerMode", false, "Enable Streamer Mode");
            BodyColor = LoadSetting("bodyColor", 0, "Player Body Color");
        }

        private static ConfigEntry<T> LoadSetting<T>(string key, T defaultValue, string description = null)
        {
            return AmongUsRevamped.Instance.Config.Bind<T>(ConfigSection, key, defaultValue, description);
        }

        /// <summary>
        /// Patch player body color with config value
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.BodyColor), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool SaveManagerBodyColorPatch(out byte __result)
        {
            __result = (byte)BodyColor.Value;
            return false;
        }
    }
}
