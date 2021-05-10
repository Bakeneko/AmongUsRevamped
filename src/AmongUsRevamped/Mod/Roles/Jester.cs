using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Jester : Role
    {
        public Jester(Player player) : base(player)
        {
            Name = "Jester";
            Faction = Faction.Neutral;
            RoleType = RoleType.Jester;
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
