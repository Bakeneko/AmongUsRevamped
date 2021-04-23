using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using HarmonyLib;
using Color = AmongUsRevamped.Colors.ColorPalette.Color;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerUpdate
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.fontSize = 2.2f;
            __instance.text.transform.localPosition = new HudPosition(2.5f, 0.3f, HudAlignment.TopRight);

            __instance.text.text = $"{Color.Revamped.ToColorTag()}{AmongUsRevamped.Name} v{AmongUsRevamped.Version}</color>";
            __instance.text.text += $"\nPing: {AmongUsClient.Instance.Ping} ms";
        }
    }
}
