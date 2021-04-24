using AmongUsRevamped.Colors;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    public class PlayerColorUtils
    {
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int VisorColor = Shader.PropertyToID("_VisorColor");

        public static float PingPong(float min, float max, float speed)
        {
            return min + Mathf.PingPong(Time.time * speed, max - min);
        }

        public static void SetRainbow(Renderer renderer)
        {
            HSBColor rainbow = new HSBColor(PingPong(0, 1, 0.3f), 1, 1);
            HSBColor rainbowShadow = new HSBColor(rainbow);
            rainbowShadow.b -= 0.3f;
            renderer.material.SetColor(BackColor, rainbowShadow.ToColor());
            renderer.material.SetColor(BodyColor, rainbow.ToColor());
            renderer.material.SetColor(VisorColor, Palette.VisorColor);
        }

        public static bool IsRainbow(StringNames name)
        {
            return name == ColorPalette.ShortColorName.Rainbow;
        }

        public static bool IsRainbow(int colorId)
        {
            try
            {
                return IsRainbow(Palette.ShortColorNames[colorId]);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRainbow(Color color)
        {
            try
            {
                return ColorPalette.Color.Rainbow.Equals(color);
            }
            catch
            {
                return false;
            }
        }
    }
}
