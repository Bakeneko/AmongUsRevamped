using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Torch : Modifier
    {
        public Torch(Player player) : base(player)
        {
            Name = "Torch";
            ModifierType = ModifierType.Torch;
            Color = ColorPalette.Color.ModifierTorch;
            VisionRangeModifier = 1.2f;
            HasNightVision = true;
            IntroDescription = () => Color.ToColorTag($"{Name}: Super vision!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Super vision!");
        }
    }
}
