using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public abstract class Role : IDisposable
    {
        private static readonly Dictionary<int, Role> Roles = new();
        private static readonly Dictionary<RoleType, List<Role>> ReverseRoles = new();

        public static List<Role> AllRoles => Roles.Values.ToList();
        public static Role GetPlayerRole(int id) => Roles.TryGetValue(id, out Role role) ? role : null;
        public static T GetPlayerRole<T>(int id) where T : Role => GetPlayerRole(id) as T;
        public static List<Role> GetRoles(RoleType type) => ReverseRoles.TryGetValue(type, out List<Role> roles) ? roles : new List<Role>();
        public static List<T> GetRoles<T>(RoleType type) where T : Role => GetRoles(type).Cast<T>().ToList();

        protected internal string Name { get; set; }
        protected internal Color Color { get; set; }
        protected internal RoleType RoleType { get; set; }
        protected internal Player Player { get; set; }
        protected internal Faction Faction { get; set; } = Faction.Crewmates;
        protected internal float MoveSpeed { get; set; } = 1f;

        protected internal float VisionRange { get; set; } = PlayerControl.GameOptions.CrewLightMod;
        protected internal bool HasNightVision { get; set; } = false;

        protected internal bool FakesTasks { get; set; } = false;

        protected internal Func<string> IntroDescription;
        protected internal Func<string> TaskDescription;
        protected internal Func<string> ExileDescription;

        public bool Exiled;

        protected bool Disposed;

        protected Role(Player player)
        {
            GetPlayerRole(player.Id)?.Dispose();
            Player = player;
            IntroDescription = () => Color.ToColorTag($"{Name}");
            TaskDescription = () => Color.ToColorTag($"{Name}");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Roles[player.Id] = this;
        }

        public virtual void OnIntroStart(IntroCutscene introCutScene, ref Il2CppSystem.Collections.Generic.List<PlayerControl> team)
        {
            // Setup team
            team.Clear();
            var currentPlayer = Player.CurrentPlayer;
            var newTeam = new List<PlayerControl>();
            switch (Faction)
            {
                case Faction.Impostors:
                    PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.IsImpostor).ToList().ForEach(p => newTeam.Add(p));
                    break;
                case Faction.Crewmates:
                    PlayerControl.AllPlayerControls.ToArray().ToList().ForEach(p => newTeam.Add(p));
                    break;
                default:
                    newTeam.Add(Player.CurrentPlayer.Control);
                    break;
            }

            // Put current player in first position
            newTeam.Sort((PlayerControl a, PlayerControl b) => {
                if (a.PlayerId == currentPlayer.Id) return -1;
                else if (b.PlayerId == PlayerControl.LocalPlayer.PlayerId) return 1;
                else return a.PlayerId - b.PlayerId;
            });

            foreach (PlayerControl player in newTeam)
            {
                team.Add(player);
            }

            // Resize Title
            introCutScene.Title.enableAutoSizing = true;
            introCutScene.Title.fontSizeMax = 14;
        }

        public virtual void OnIntroUpdate(IntroCutscene.Nested_0 introCutScene)
        {
            introCutScene.__this.Title.text = Color.ToColorTag(Name);
            introCutScene.__this.ImpostorText.text = IntroDescription();
            introCutScene.__this.ImpostorText.gameObject.SetActive(true);
            introCutScene.__this.BackgroundBar.material.color = Color;
        }

        public virtual void HudUpdate(HudManager hudManager)
        {

        }

        public virtual void CurrentPlayerHudUpdate(HudManager hudManager)
        {

        }

        public virtual void OnExiled()
        {
            Exiled = true;
        }

        public virtual void OnMurdered(Player killer)
        {

        }

        public virtual void OnEnd(Game.GameOverData gameOver)
        {
            switch (gameOver.Reason)
            {
                case Game.CustomGameOverReason.JesterVotedOut:
                    gameOver.Text = "Jester wins";
                    gameOver.BackgroundColor = gameOver.TextColor = ColorPalette.Color.RoleJester;
                    break;
            }

            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (Player player in gameOver.Winners)
            {
                TempData.winners.Add(new WinningPlayerData(player.Data));
            }
        }

        protected internal virtual bool CanCallMeeting()
        {
            return true;
        }

        protected internal virtual bool CanUseVent(Vent vent)
        {
            return false;
        }

        protected internal virtual bool CanSeeRole(Player other)
        {
            return false;
        }

        public void AddToReverseIndex()
        {
            RemoveFromReverseIndex();
            var reverse = GetRoles(RoleType);
            reverse.Add(this);
            ReverseRoles[RoleType] = reverse;
        }

        public void RemoveFromReverseIndex()
        {
            GetRoles(RoleType).Remove(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Role)) return false;
            return Equals((Role)obj);
        }

        private bool Equals(Role other)
        {
            return Player.Id == other.Player.Id && RoleType == other.RoleType;
        }

        public static bool operator == (Role a, Role b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator != (Role a, Role b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Player.Id, (byte)RoleType);
        }

        /// <summary>
        /// Clean up
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;

            try
            {
                RemoveFromReverseIndex();
                Roles.Remove(Player.Id);
                Player.UpdateImportantTasks();
            }
            catch
            {
            }

            Disposed = true;
        }

        /// <summary>
        /// Clean up
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public enum Faction
    {
        Neutral,
        Crewmates,
        Impostors
    }

    public enum RoleType
    {
        // Default
        None,
        // Crew
        Crewmate,
        Sheriff,
        // Impostor
        Impostor,
        // Neutral
        Jester,
    }
}
