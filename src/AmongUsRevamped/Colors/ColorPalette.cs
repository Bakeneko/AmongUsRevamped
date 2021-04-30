using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using UnityEngine;
using PaletteColors = AmongUsRevamped.Colors.ColorPalette.Color.Palette;

namespace AmongUsRevamped.Colors
{
    /// <summary>
    /// Color wrapper
    /// </summary>
    public static class ColorPalette
    {
        public class PaletteColor
        {
            public static Dictionary<int, string> ColorNames = new();

            // Vanilla colors
            public static PaletteColor Red = new("RED", "Red", PaletteColors.Red, PaletteColors.RedShadow);
            public static PaletteColor Blue = new("BLUE", "Blue", PaletteColors.Blue, PaletteColors.BlueShadow);
            public static PaletteColor Green = new("GRN", "Green", PaletteColors.Green, PaletteColors.GreenShadow);
            public static PaletteColor Pink = new("PINK", "Pink", PaletteColors.Pink, PaletteColors.PinkShadow);
            public static PaletteColor Orange = new("ORGN", "Orange", PaletteColors.Orange, PaletteColors.OrangeShadow);
            public static PaletteColor Yellow = new("YLOW", "Yellow", PaletteColors.Yellow, PaletteColors.YellowShadow);
            public static PaletteColor Black = new("BLAK", "Black", PaletteColors.Black, PaletteColors.BlackShadow);
            public static PaletteColor White = new("WHTE", "White", PaletteColors.White, PaletteColors.WhiteShadow);
            public static PaletteColor Purple = new("PURP", "Purple", PaletteColors.Purple, PaletteColors.PurpleShadow);
            public static PaletteColor Brown = new("BRWN", "Brown", PaletteColors.Brown, PaletteColors.BrownShadow);
            public static PaletteColor Cyan = new("CYAN", "Cyan", PaletteColors.Cyan, PaletteColors.CyanShadow);
            public static PaletteColor Lime = new("LIME", "Lime", PaletteColors.Lime, PaletteColors.LimeShadow);

            // New colours
            public static PaletteColor Mint = new("MINT", "Mint", PaletteColors.Mint, PaletteColors.MintShadow);
            public static PaletteColor Salmon = new("SALMN", "Salmon", PaletteColors.Salmon, PaletteColors.SalmonShadow);
            public static PaletteColor Nougat = new("NOUGT", "Nougat", PaletteColors.Nougat, PaletteColors.NougatShadow);
            public static PaletteColor Bordeaux = new("BRDX", "Bordeaux", PaletteColors.Bordeaux, PaletteColors.BordeauxShadow);
            public static PaletteColor Lavender = new("LVNDR", "Lavender", PaletteColors.Lavender, PaletteColors.LavenderShadow);
            public static PaletteColor Wasabi = new("WSBI", "Wasabi", PaletteColors.Wasabi, PaletteColors.WasabiShadow);
            public static PaletteColor Turqoise = new("TURQ", "Turqoise", PaletteColors.Turquoise, PaletteColors.TurquoiseShadow);
            public static PaletteColor HotPink = new("HPNK", "Hot Pink", PaletteColors.HotPink, PaletteColors.HotPinkShadow);
            public static PaletteColor Petrol = new("PTRL", "Petrol", PaletteColors.Petrol, PaletteColors.PetrolShadow);
            public static PaletteColor Amber = new("AMBR", "Amber", PaletteColors.Amber, PaletteColors.AmberShadow);
            public static PaletteColor Gray = new("GRAY", "Gray", PaletteColors.Gray, PaletteColors.GrayShadow);
            public static PaletteColor Rainbow = new("RNBW", "Rainbow", PaletteColors.Rainbow, PaletteColors.Rainbow);

            public string ShortName;
            public string LongName;
            public Color32 Color;
            public Color32 Shadow;

            public PaletteColor(string shortname, string longName, Color32 color, Color32 shadow)
            {
                ShortName = shortname;
                LongName = longName;
                Color = color;
                Shadow = shadow;
            }

            public string ToColorTag(string text)
            {
                return Color.ToColorTag(text);
            }
        }

        public static class Color
        {
            public static class Palette
            {
                // Vanilla colors
                public static Color32 Red = new(198, 17, 17, 255);
                public static Color32 RedShadow = new(122, 8, 56, 255);
                public static Color32 Blue = new(19, 46, 210, 255);
                public static Color32 BlueShadow = new(9, 21, 142, 255);
                public static Color32 Green = new(17, 128, 45, 255);
                public static Color32 GreenShadow = new(10, 77, 46, 255);
                public static Color32 Pink = new(238, 84, 187, 255);
                public static Color32 PinkShadow = new(172, 43, 174, 255);
                public static Color32 Orange = new(240, 125, 13, 255);
                public static Color32 OrangeShadow = new(180, 62, 21, 255);
                public static Color32 Yellow = new(246, 246, 87, 255);
                public static Color32 YellowShadow = new(195, 136, 34, 255);
                public static Color32 Black = new(63, 71, 78, 255);
                public static Color32 BlackShadow = new(30, 31, 38, 255);
                public static Color32 White = new(215, 225, 241, 255);
                public static Color32 WhiteShadow = new(132, 149, 192, 255);
                public static Color32 Purple = new(107, 47, 188, 255);
                public static Color32 PurpleShadow = new(59, 23, 124, 255);
                public static Color32 Brown = new(113, 73, 30, 255);
                public static Color32 BrownShadow = new(94, 38, 21, 255);
                public static Color32 Cyan = new(56, 255, 221, 255);
                public static Color32 CyanShadow = new(36, 169, 191, 255);
                public static Color32 Lime = new(80, 240, 57, 255);
                public static Color32 LimeShadow = new(21, 168, 66, 255);

                // New colours
                public static Color32 Mint = new(111, 192, 156, 255);
                public static Color32 MintShadow = new(65, 148, 111, 255);
                public static Color32 Salmon = new(239, 191, 192, 255);
                public static Color32 SalmonShadow = new(182, 119, 114, 255);
                public static Color32 Nougat = new(160, 101, 56, 255);
                public static Color32 NougatShadow = new(109, 69, 38, 255);
                public static Color32 Bordeaux = new(109, 7, 26, 255);
                public static Color32 BordeauxShadow = new(54, 2, 11, 255);
                public static Color32 Lavender = new(173, 126, 201, 255);
                public static Color32 LavenderShadow = new(131, 58, 203, 255);
                public static Color32 Wasabi = new(112, 143, 46, 255);
                public static Color32 WasabiShadow = new(72, 92, 29, 255);
                public static Color32 Turquoise = new(22, 132, 176, 255);
                public static Color32 TurquoiseShadow = new(15, 89, 117, 255);
                public static Color32 HotPink = new(255, 51, 102, 255);
                public static Color32 HotPinkShadow = new(232, 0, 58, 255);
                public static Color32 Petrol = new(0, 99, 105, 255);
                public static Color32 PetrolShadow = new(0, 61, 54, 255);
                public static Color32 Amber = new(255, 191, 0, 255);
                public static Color32 AmberShadow = new(179, 134, 0, 255);
                public static Color32 Gray = new(147, 147, 147, 255);
                public static Color32 GrayShadow = new(120, 120, 120, 255);
                public static Color32 Rainbow = new(0, 0, 0, 255);
            }

            public static Color32 Revamped = new(255, 191, 0, 255);

            public static Color32 SettingGreen = new(0, 255, 42, 255);

            public static Color32 Success = new(0, 255, 0, 255);
            public static Color32 Warning = new(255, 255, 0, 255);
            public static Color32 Error = new(255, 0, 0, 255);

            public static Color32 TasksIncomplete = new(255, 255, 0, 255);
            public static Color32 TasksComplete = new(0, 221, 0, 255);
        }
    }
}
