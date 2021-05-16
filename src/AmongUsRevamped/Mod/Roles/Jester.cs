using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Jester : Crewmate
    {
        public Jester(Player player) : base(player, RoleType.Jester)
        {
            Name = "Jester";
            Faction = Faction.Neutral;
            Color = ColorPalette.Color.RoleJester;
            FakesTasks = true;
            IntroDescription = () => "Get voted out!";
            TaskDescription = () => Color.ToColorTag($"{Name}: Get voted out!");
            ExileDescription = () => $"{Player.Name} was The {Name}";
        }

        protected internal override bool CanCallMeeting()
        {
            return false;
        }
    }
}
