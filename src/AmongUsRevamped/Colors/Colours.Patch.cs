using HarmonyLib;
using UnhollowerBaseLib;

namespace AmongUsRevamped.Colors
{
    /// <summary>
    /// Resolve palette added color names
    /// </summary>
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
         new[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public class PatchColours
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            switch (name)
            {
                case ColorPalette.ShortColorName.Watermelon:
                    __result = "MELON";
                    return false;
                case ColorPalette.ShortColorName.Chocolate:
                    __result = "CHOCO";
                    return false;
                case ColorPalette.ShortColorName.SkyBlue:
                    __result = "LTBL";
                    return false;
                case ColorPalette.ShortColorName.Beige:
                    __result = "BEIGE";
                    return false;
                case ColorPalette.ShortColorName.HotPink:
                    __result = "LTPNK";
                    return false;
                case ColorPalette.ShortColorName.Turquoise:
                    __result = "TURQ";
                    return false;
                case ColorPalette.ShortColorName.Lilac:
                    __result = "LILAC";
                    return false;
                case ColorPalette.ShortColorName.Amber:
                    __result = "AMBER";
                    return false;
                case ColorPalette.ShortColorName.Rainbow:
                    __result = "RNBW";
                    return false;
                case ColorPalette.ColorName.Watermelon:
                    __result = "Watermelon";
                    return false;
                case ColorPalette.ColorName.Chocolate:
                    __result = "Chocolate";
                    return false;
                case ColorPalette.ColorName.SkyBlue:
                    __result = "Sky Blue";
                    return false;
                case ColorPalette.ColorName.Beige:
                    __result = "Beige";
                    return false;
                case ColorPalette.ColorName.HotPink:
                    __result = "Hot Pink";
                    return false;
                case ColorPalette.ColorName.Turquoise:
                    __result = "Turquoise";
                    return false;
                case ColorPalette.ColorName.Lilac:
                    __result = "Lilac";
                    return false;
                case ColorPalette.ColorName.Amber:
                    __result = "Amber";
                    return false;
                case ColorPalette.ColorName.Rainbow:
                    __result = "Rainbow";
                    return false;
            }

            return true;
        }
    }
}
