using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public class Drunk : Modifier
    {
        public Drunk(Player player) : base(player)
        {
            Name = "Drunk";
            ModifierType = ModifierType.Drunk;
            Color = ColorPalette.Color.ModifierDrunk;
            MoveSpeedModifier = -1f;
            IntroDescription = () => Color.ToColorTag($"{Name}: Go home, you're drunk!");
            TaskDescription = () => Color.ToColorTag($"{Name}: Go home, you're drunk!");
        }
    }
}
