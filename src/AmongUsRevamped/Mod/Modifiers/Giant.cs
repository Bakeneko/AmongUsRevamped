using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Giant : Modifier
    {
        public Giant(Player player) : base(player, ModifierType.Giant)
        {
            Name = "Giant";
            Color = ColorPalette.Color.ModifierGiant;
            MoveSpeedModifier = 0.75f;
            SizeModifier = 1.3f;
            IntroDescription = () => Color.ToColorTag($"{Name}: Mind the ceiling!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Mind the ceiling!");
        }
    }
}
