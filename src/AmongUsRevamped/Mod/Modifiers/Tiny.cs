using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Tiny : Modifier
    {
        public Tiny(Player player) : base(player, ModifierType.Tiny)
        {
            Name = "Tiny";
            Color = ColorPalette.Color.ModifierTiny;
            MoveSpeedModifier = 1.25f;
            SizeModifier = 0.7f;
            VisionRangeModifier = 0.8f;
            IntroDescription = () => Color.ToColorTag($"{Name}: Small but nimble!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Small but nimble!");
        }
    }
}
