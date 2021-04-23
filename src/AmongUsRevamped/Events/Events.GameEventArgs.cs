using System;

namespace AmongUsRevamped.Events
{
    public static partial class GameEvents
    {
        public class VentEventArgs : EventArgs
        {
            public int Id;
            public string Name;
            public readonly SystemTypes System;
            public readonly PlayerControl Player;

            public VentEventArgs(int id, string name, SystemTypes system, PlayerControl player)
            {
                Id = id;
                Name = name;
                Player = player;
                System = system;
            }
        }
    }
}
