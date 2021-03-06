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
        protected internal float Size { get; set; } = 1f;

        protected internal float VisionRange { get; set; } = PlayerControl.GameOptions.CrewLightMod;
        protected internal bool HasNightVision { get; set; } = false;

        protected internal bool FakesTasks { get; set; } = false;

        protected internal Func<string> IntroDescription;
        protected internal Func<string> TaskDescription;
        protected internal Func<string> ExileDescription;

        public Disguise Disguise;
        public bool Exiled;

        protected bool Disposed;

        protected Role(Player player, RoleType roleType)
        {
            GetPlayerRole(player.Id)?.Dispose();
            Player = player;
            RoleType = roleType;
            IntroDescription = () => Color.ToColorTag($"{Name}");
            TaskDescription = () => Color.ToColorTag($"{Name}");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Roles[player.Id] = this;
            AddToReverseIndex();
            UpdateVentOutlines();
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
                    AllRoles.Where(r => r.Faction == Faction.Impostors || r.RoleType == RoleType.Spy).ToList().ForEach(r => newTeam.Add(r.Player.Control));
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

        public virtual void OnIntroUpdate(IntroCutscene introCutScene)
        {
            introCutScene.Title.text = Color.ToColorTag(Name);
            introCutScene.ImpostorText.text = IntroDescription();
            introCutScene.ImpostorText.gameObject.SetActive(true);
            introCutScene.BackgroundBar.material.color = Color;
        }

        public virtual void OnIntroEnd(IntroCutscene introCutScene)
        {
            UpdateVentOutlines();
        }

        public virtual void HudUpdate(HudManager hudManager)
        {

        }

        public virtual void CurrentPlayerHudUpdate(HudManager hudManager)
        {

        }

        protected virtual void UpdateVentOutlines()
        {
            if (!Player.IsCurrentPlayer || ShipStatus.Instance?.AllVents == null) return;

            try
            {
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    vent.myRend.SetOutline(Player.CanUseVent(vent) ? Color : null);
                }
            }
            catch { }
        }

        public virtual void OnExiled()
        {
            Exiled = true;
            Disguise = null;
            ApplyDisguise();
        }

        public virtual void OnExileEnd(ExileController exileController)
        {

        }

        public virtual void OnMurdered(Player killer)
        {
            Disguise = null;
            ApplyDisguise();
        }

        public virtual void OnRevived()
        {

        }

        public virtual void OnTasksCreated()
        {

        }

        public virtual void OnCompletedTask(GameData.TaskInfo task)
        {

        }

        public virtual void ApplyDisguise()
        {
            var control = Player?.Control;
            if (control == null) return;

            if (Disguise != null) {

                control.nameText.text = Disguise.Name;

                if (control.myRend != null)
                {
                    if (Disguise.ColorId != -1) PlayerControl.SetPlayerMaterialColors(Disguise.ColorId, control.myRend);
                    if (Disguise.Color != default) PlayerControl.SetPlayerMaterialColors(Disguise.Color, control.myRend);
                    control.myRend.color = Disguise.BodyRenderColor;
                }

                if (control.HatRenderer != null)
                {
                    if (Disguise.ColorId != -1) control.HatRenderer.SetHat(Disguise.HatId, Disguise.ColorId);
                    control.HatRenderer.FrontLayer.color = Disguise.OtherRenderColor;
                    control.HatRenderer.BackLayer.color = Disguise.OtherRenderColor;
                }

                if (control.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>.Instance
                .AllSkins.ToArray()[(int)Disguise.SkinId].ProdId)
                {
                    Player.SetSkin((int)Disguise.SkinId);
                }
                if (control.MyPhysics.Skin != null) control.MyPhysics.Skin.layer.color = Disguise.OtherRenderColor;

                if (control.CurrentPet == null || control.CurrentPet.ProdId !=
                DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[(int)Disguise.PetId].ProdId)
                {
                    if (control.CurrentPet != null) control.CurrentPet.gameObject.Destroy();

                    control.CurrentPet = UnityEngine.Object.Instantiate(DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[(int)Disguise.PetId]);
                    control.CurrentPet.transform.position = control.transform.position;
                    control.CurrentPet.Source = control;
                    control.CurrentPet.Visible = Player.Visible;
                }
                if (control.CurrentPet != null)
                {
                    control.CurrentPet.rend.color = Disguise.PetRenderColor;
                    control.CurrentPet.shadowRend.color = Disguise.PetRenderColor;
                }

                Player.UpdateSize();
            }
            else
            {
                control.nameText.text = Player.Data.PlayerName;
                var renderColor = Color.white;

                var colorId = Player.Data.ColorId;
                if (control.myRend != null)
                {
                    PlayerControl.SetPlayerMaterialColors(colorId, control.myRend);
                    control.myRend.color = renderColor;
                }

                if (control.HatRenderer != null)
                {
                    control.HatRenderer.SetHat(Player.Data.HatId, colorId);
                    control.HatRenderer.FrontLayer.color = renderColor;
                    control.HatRenderer.BackLayer.color = renderColor;
                }

                if (control.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>.Instance
                .AllSkins.ToArray()[(int)Player.Data.SkinId].ProdId)
                {
                    Player.SetSkin((int)Player.Data.SkinId);
                }
                if (control.MyPhysics.Skin != null) control.MyPhysics.Skin.layer.color = renderColor;

                if (control.CurrentPet == null || control.CurrentPet.ProdId !=
                DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[(int)Player.Data.PetId].ProdId)
                {
                    if (control.CurrentPet != null) control.CurrentPet.gameObject.Destroy();

                    control.CurrentPet = UnityEngine.Object.Instantiate(DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[(int)Player.Data.PetId]);
                    control.CurrentPet.transform.position = control.transform.position;
                    control.CurrentPet.Source = control;
                    control.CurrentPet.Visible = Player.Visible;
                }
                if (control.CurrentPet != null)
                {
                    control.CurrentPet.rend.color = renderColor;
                    control.CurrentPet.shadowRend.color = renderColor;
                }

                Player.UpdateSize();
            }
        }

        public virtual void OnEnd(Game.GameOverData gameOver)
        {
            if (gameOver == null) return;

            switch (gameOver.Reason)
            {
                case Game.CustomGameOverReason.JesterVotedOut:
                    gameOver.Text = "Jester wins";
                    gameOver.BackgroundColor = gameOver.TextColor = ColorPalette.Color.RoleJester;
                    break;
            }

            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();

            if (gameOver.Winners == null) return;

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
                Exiled = false;
                RemoveFromReverseIndex();
                Roles.Remove(Player.Id);
                Player?.UpdateImportantTasks();
                Player = null;
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

    public class Disguise
    {
        public string Name = "";
        public int ColorId = -1;
        public Color Color = default;
        public uint HatId;
        public uint SkinId;
        public uint PetId;
        public float Size = 1f;
        public float MoveSpeed = 1f;
        public Color BodyRenderColor = Color.white;
        public Color PetRenderColor = Color.white;
        public Color OtherRenderColor = Color.white;

        public Disguise(string name, int colorId, Color color, uint hatId, uint skinId, uint petId, float size = 1f, float moveSpeed = 1f)
        {
            Name = name;
            ColorId = colorId;
            Color = color;
            HatId = hatId;
            SkinId = skinId;
            PetId = petId;
            Size = size;
            MoveSpeed = moveSpeed;
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
        Engineer,
        Sheriff,
        Snitch,
        Spy,
        TimeLord,
        // Impostor
        Camouflager,
        Cleaner,
        Impostor,
        Morphling,
        Swooper,
        // Neutral
        Jester,
    }
}
