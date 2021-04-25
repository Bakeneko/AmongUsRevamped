using UnityEngine;

namespace AmongUsRevamped.Colors
{
    /// <summary>
    /// Color wrapper
    /// </summary>
    public static class ColorPalette
    {
        public static class ShortColorName
        {
            // Vanilla colors
            public const StringNames Red = StringNames.VitalsRED;
            public const StringNames Blue = StringNames.VitalsBLUE;
            public const StringNames Green = StringNames.VitalsGRN;
            public const StringNames Pink = StringNames.VitalsPINK;
            public const StringNames Orange = StringNames.VitalsORGN;
            public const StringNames Yellow = StringNames.VitalsYLOW;
            public const StringNames Black = StringNames.VitalsBLAK;
            public const StringNames White = StringNames.VitalsWHTE;
            public const StringNames Purple = StringNames.VitalsPURP;
            public const StringNames Brown = StringNames.VitalsBRWN;
            public const StringNames Cyan = StringNames.VitalsCYAN;
            public const StringNames Lime = StringNames.VitalsLIME;
            // New colors
            public const StringNames Watermelon = (StringNames)999999;
            public const StringNames Chocolate = (StringNames)999998;
            public const StringNames SkyBlue = (StringNames)999997;
            public const StringNames Beige = (StringNames)999996;
            public const StringNames HotPink = (StringNames)999995;
            public const StringNames Turquoise = (StringNames)999994;
            public const StringNames Lilac = (StringNames)999993;
            public const StringNames Amber = (StringNames)999992;
            public const StringNames Rainbow = (StringNames)999991;
        }

        public static class ColorName
        {
            // Vanilla colors
            public const StringNames Red = StringNames.ColorRed;
            public const StringNames Blue = StringNames.ColorBlue;
            public const StringNames Green = StringNames.ColorGreen;
            public const StringNames Pink = StringNames.ColorPink;
            public const StringNames Orange = StringNames.ColorOrange;
            public const StringNames Yellow = StringNames.ColorYellow;
            public const StringNames Black = StringNames.ColorBlack;
            public const StringNames White = StringNames.ColorWhite;
            public const StringNames Purple = StringNames.ColorPurple;
            public const StringNames Brown = StringNames.ColorBrown;
            public const StringNames Cyan = StringNames.ColorCyan;
            public const StringNames Lime = StringNames.ColorLime;
            // New colors
            public const StringNames Watermelon = (StringNames)899999;
            public const StringNames Chocolate = (StringNames)899998;
            public const StringNames SkyBlue = (StringNames)899997;
            public const StringNames Beige = (StringNames)899996;
            public const StringNames HotPink = (StringNames)899995;
            public const StringNames Turquoise = (StringNames)899994;
            public const StringNames Lilac = (StringNames)899993;
            public const StringNames Amber = (StringNames)899992;
            public const StringNames Rainbow = (StringNames)899991;
        }

        public static class Color
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
            public static Color32 Watermelon = new(168, 50, 62, 255);
            public static Color32 WatermelonShadow = new(101, 30, 37, 255);
            public static Color32 Chocolate = new(60, 48, 44, 255);
            public static Color32 ChocolateShadow = new(30, 24, 22, 255);
            public static Color32 SkyBlue = new(61, 129, 255, 255);
            public static Color32 SkyBlueShadow = new(31, 65, 128, 255);
            public static Color32 Beige = new(240, 211, 165, 255);
            public static Color32 BeigeShadow = new(120, 106, 83, 255);
            public static Color32 HotPink = new(236, 61, 255, 255);
            public static Color32 HotPinkShadow = new(118, 31, 128, 255);
            public static Color32 Turquoise = new(61, 255, 181, 255);
            public static Color32 TurquoiseShadow = new(31, 128, 91, 255);
            public static Color32 Lilac = new(186, 161, 255, 255);
            public static Color32 LilacShadow = new(93, 81, 128, 255);
            public static Color32 Amber = new(255, 191, 0, 255);
            public static Color32 AmberShadow = new(179, 134, 0, 255);
            public static Color32 Rainbow = new(0, 0, 0, 255);
            public static Color32 RainbowShadow = new(0, 0, 0, 255);

            // Mod color
            public static Color32 Revamped = new(255, 191, 0, 255);
        }
    }
}
