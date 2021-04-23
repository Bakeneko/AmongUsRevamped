using AmongUsRevamped.Utils;
using UnityEngine;

namespace AmongUsRevamped.Extensions
{
    public static class ColorExtensions
    {
        public static Color32 ToColor32(this Color color)
        {
            return ColorUtils.ToColor32(color);
        }

        public static string ToHtmlStringRGB(this Color color)
        {
            return ColorUtils.ToHtmlStringRGB(color);
        }

        public static string ToHtmlStringRGB(this Color32 color)
        {
            return ColorUtils.ToHtmlStringRGB(color);
        }

        public static string ToHtmlStringRGBA(this Color color)
        {
            return ColorUtils.ToHtmlStringRGBA(color);
        }

        public static string ToHtmlStringRGBA(this Color32 color)
        {
            return ColorUtils.ToHtmlStringRGBA(color);
        }

        public static string ToColorTag(this Color color)
        {
            return $"<color={ColorUtils.ToHtmlStringRGBA(color)}>";
        }

        public static string ToColorTag(this Color32 color)
        {
            return $"<color={ColorUtils.ToHtmlStringRGBA(color)}>";
        }
    }
}
