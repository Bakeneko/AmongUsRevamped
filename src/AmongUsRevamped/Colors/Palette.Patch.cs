using System.Collections.Generic;
using AmongUsRevamped.Options;
using Assets.CoreScripts;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

using PaletteColor = AmongUsRevamped.Colors.ColorPalette.PaletteColor;

namespace AmongUsRevamped.Colors
{
    [HarmonyPatch]
    public static class PalettePatch
    {
        public static void Load()
        {
            List<PaletteColor> palette = new()
            {
                PaletteColor.Red,
                PaletteColor.Blue,
                PaletteColor.Green,
                PaletteColor.Pink,
                PaletteColor.Orange,
                PaletteColor.Yellow,
                PaletteColor.Black,
                PaletteColor.White,
                PaletteColor.Purple,
                PaletteColor.Brown,
                PaletteColor.Cyan,
                PaletteColor.Lime,
                PaletteColor.Mint,
                PaletteColor.Salmon,
                PaletteColor.Nougat,
                PaletteColor.Bordeaux,
                PaletteColor.Lavender,
                PaletteColor.Wasabi,
                PaletteColor.Turqoise,
                PaletteColor.HotPink,
                PaletteColor.Petrol,
                PaletteColor.Amber,
                PaletteColor.Gray,
                PaletteColor.Rainbow
            };

            List<StringNames> shortColorNames = new();
            List<StringNames> colorNames = new();

            List<Color32> playerColors = new();
            List<Color32> shadowColors = new();

            int id = 900000;
            foreach (PaletteColor col in palette)
            {
                shortColorNames.Add((StringNames)id);
                colorNames.Add((StringNames)id);

                playerColors.Add(col.Color);
                shadowColors.Add(col.Shadow);

                PaletteColor.ColorNames[id++] = col.ShortName;
                PaletteColor.ColorNames[id++] = col.LongName;
            }

            Palette.ShortColorNames = shortColorNames.ToArray();
            Palette.PlayerColors = playerColors.ToArray();
            Palette.ShadowColors = shadowColors.ToArray();
            MedScanMinigame.ColorNames = colorNames.ToArray();
            Telemetry.ColorNames = colorNames.ToArray();
        }


        /// <summary>
        /// Resolve palette color names
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
        new[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) }
        )]
        public static bool TranslationControllerGetStringPatch(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            return !PaletteColor.ColorNames.TryGetValue((int)name, out __result);
        }

        /// <summary>
        /// Dynamically position color chips
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        public static void PlayerTabOnEnablePatch(PlayerTab __instance)
        {
            Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

            float left = 1.4f;
            float top = -0.35f;
            float right = 3.89f;
            float bottom = -3.84f;
            float width = right - left;
            float height = top - bottom;
            float baseScale = 1.3f;
            float baseCols = 3.0f;
            float baseRows = 5.0f;
            float ratio = 0.714f;
            
            int rows = (int)Mathf.Ceil(Mathf.Sqrt(chips.Length / ratio ));
            int cols = (int) Mathf.Ceil(chips.Length / (float) rows);
 
            float scale = Mathf.Min(baseCols / cols, baseRows / rows) * baseScale;
            float chipSize = 0.565f;
            float chipMargin = Mathf.Min((width - cols * chipSize) / (cols + 1), (height - rows * chipSize) / (rows + 1));

            float horizPadding = (width - cols * (chipSize + chipMargin) - chipMargin) / 2;
            float vertPadding = (height - rows * (chipSize + chipMargin) - chipMargin) / 2;

            for (int i = 0; i < chips.Length; i++)
            {
                ColorChip chip = chips[i];
                int row = i / cols, col = i % cols;
                chip.transform.localPosition = new Vector3(left + horizPadding + chipMargin + (col * (chipSize + chipMargin)), top - vertPadding - chipMargin - (row * (chipSize + chipMargin)), chip.transform.localPosition.z);
                chip.transform.localScale = new Vector3(scale, scale, 1.0f);
            }
        }

        /// <summary>
        /// Persist selected body color
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.SelectColor))]
        public static void PlayerTabSelectColorPatch([HarmonyArgument(0)] int colorIndex)
        {
            CustomSettings.BodyColor.Value = colorIndex;
            // Only save vanilla colors 
            SaveManager.BodyColor = (byte)(colorIndex < 12 ? colorIndex : 0);
        }
    }
}
