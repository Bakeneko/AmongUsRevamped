using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod.Modifiers
{
    public abstract class Modifier: IDisposable
    {
        private static readonly Dictionary<int, Modifier> Modifiers = new();
        private static readonly Dictionary<ModifierType, List<Modifier>> ReverseModifiers = new();

        public static List<Modifier> AllModifiers => Modifiers.Values.ToList();
        public static Modifier GetPlayerModifier(int id) => Modifiers.TryGetValue(id, out Modifier modifier) ? modifier : null;
        public static T GetPlayerModifier<T>(int id) where T : Modifier => GetPlayerModifier(id) as T;
        public static List<Modifier> GetModifiers(ModifierType type) => ReverseModifiers.TryGetValue(type, out List<Modifier> modifiers) ? modifiers : new List<Modifier>();
        public static List<T> GetModifiers<T>(ModifierType type) where T : Modifier => GetModifiers(type).Cast<T>().ToList();

        protected internal string Name { get; set; }
        protected internal Color Color { get; set; }
        protected internal ModifierType ModifierType { get; set; }
        protected internal Player Player { get; set; }
        protected internal float MoveSpeedModifier { get; set; } = 1f;
        protected internal float SizeModifier { get; set; } = 1f;
        protected internal float VisionRangeModifier { get; set; } = 1f;
        protected internal bool HasNightVision { get; set; } = false;

        protected internal Func<string> IntroDescription;
        protected internal Func<string> TaskDescription;

        protected bool Disposed;

        protected Modifier(Player player)
        {
            GetPlayerModifier(player.Id)?.Dispose();
            Player = player;
            IntroDescription = () => Color.ToColorTag($"{Name}");
            TaskDescription = () => Color.ToColorTag($"{Name}");
            Modifiers[player.Id] = this;
        }

        public virtual void OnIntroStart(IntroCutscene introCutScene)
        {
            // Retrieve or instantiate modifier text
            Transform modifierTextTransform = introCutScene.Title.transform.parent.transform?.FindChild("ModifierText");
            TMPro.TextMeshPro modifierText = modifierTextTransform?.GetComponent<TMPro.TextMeshPro>();
            if (modifierText == null)
            {
                modifierText = UnityEngine.Object.Instantiate(introCutScene.Title, introCutScene.Title.transform.parent, false);
                modifierText.enableAutoSizing = true;
                modifierText.fontSizeMax = 5;
                modifierText.gameObject.name = "ModifierText";
                modifierText.gameObject.SetActive(false);
            }
        }

        public virtual void OnIntroUpdate(IntroCutscene introCutScene)
        {
            // Retrieve modifier text
            Transform modifierTextTransform = introCutScene.Title.transform.parent.transform?.FindChild("ModifierText");
            TMPro.TextMeshPro modifierText = modifierTextTransform?.GetComponent<TMPro.TextMeshPro>();

            if (modifierText != null)
            {
                modifierText.text = IntroDescription();
                modifierText.transform.position = introCutScene.transform.position - new Vector3(0f, 1.8f, 0f);
                modifierText.gameObject.SetActive(true);
            }
        }

        public virtual void OnIntroEnd(IntroCutscene introCutScene)
        {

        }

        public virtual void HudUpdate(HudManager hudManager)
        {

        }

        public virtual void CurrentPlayerHudUpdate(HudManager hudManager)
        {

        }

        public void AddToReverseIndex()
        {
            RemoveFromReverseIndex();
            var reverse = GetModifiers(ModifierType);
            reverse.Add(this);
            ReverseModifiers[ModifierType] = reverse;
        }

        public void RemoveFromReverseIndex()
        {
            GetModifiers(ModifierType).Remove(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Modifier)) return false;
            return Equals((Modifier)obj);
        }

        private bool Equals(Modifier other)
        {
            return Player.Id == other.Player.Id && ModifierType == other.ModifierType;
        }

        public static bool operator == (Modifier a, Modifier b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator != (Modifier a, Modifier b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Player.Id, (byte)ModifierType);
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
                Modifiers.Remove(Player.Id);
                Player.UpdateImportantTasks();
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

    public enum ModifierType
    {
        // Default
        None,
        Drunk,
        Flash,
        Giant,
        Torch,
    }
}
