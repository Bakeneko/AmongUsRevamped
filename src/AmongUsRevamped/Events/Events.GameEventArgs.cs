using System;

namespace AmongUsRevamped.Events
{
    public static partial class GameEvents
    {
        public class VoteCastedEventArgs : EventArgs
        {
            public readonly GameData.PlayerInfo Player;
            public readonly GameData.PlayerInfo VotedPlayer;

            public VoteCastedEventArgs(GameData.PlayerInfo player, GameData.PlayerInfo votedPlayer)
            {
                Player = player;
                VotedPlayer = votedPlayer;
            }
        }

        public class VotingCompletedEventArgs : EventArgs
        {
            public readonly GameData.PlayerInfo EjectedPlayer;

            public VotingCompletedEventArgs(GameData.PlayerInfo ejectedPlayer)
            {
                EjectedPlayer = ejectedPlayer;
            }
        }

        public class VentEventArgs : EventArgs
        {
            public int Id;
            public string Name;
            public readonly SystemTypes System;
            public readonly PlayerControl Player;

            public VentEventArgs(SystemTypes system, PlayerControl player)
            {
                Player = player;
                System = system;
            }
        }
    }
}
