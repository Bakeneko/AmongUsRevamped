using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Crewmate : Role
    {
        public Crewmate(Player player) : base(player)
        {
            Name = "Crewmate";
            Faction = Faction.Crewmates;
            RoleType = RoleType.Crewmate;
            Color = ColorPalette.Color.RoleCrewmate;
            IntroDescription = () => $"Find the {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")}";
            TaskDescription = () => Color.ToColorTag($"{Name}: Find the Impostors");
            ExileDescription = () => $"{Player.Name} was a {Name}";
        }
    }
}
