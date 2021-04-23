using UnityEngine;

namespace AmongUsRevamped.Utils
{
    public static class ColorUtils
    {

        public static Color32 ToColor32(Color color)
        {
            // Round to int to prevent precision issues that, for example cause values very close to 1 to become FE instead of FF (case 770904).
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255),
                1);
        }

        public static string ToHtmlStringRGB(Color color)
        {
            return ToHtmlStringRGB(ToColor32(color));
        }

        public static string ToHtmlStringRGB(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }

        public static string ToHtmlStringRGBA(Color color)
        {
            return ToHtmlStringRGBA(ToColor32(color));
        }

        public static string ToHtmlStringRGBA(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
        }
    }
}
