using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Flash : Modifier
    {
        public Flash(Player player) : base(player)
        {
            Name = "Flash";
            ModifierType = ModifierType.Flash;
            Color = ColorPalette.Color.ModifierFlash;
            MoveSpeed = 2f;
            IntroDescription = () => Color.ToColorTag($"{Name}: Zooooom!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Zooooom!");
        }
    }
}
