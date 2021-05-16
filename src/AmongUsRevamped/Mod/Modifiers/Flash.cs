using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Flash : Modifier
    {
        public Flash(Player player) : base(player, ModifierType.Flash)
        {
            Name = "Flash";
            Color = ColorPalette.Color.ModifierFlash;
            MoveSpeedModifier = 1.5f;
            IntroDescription = () => Color.ToColorTag($"{Name}: Zooooom!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Zooooom!");
        }
    }
}
