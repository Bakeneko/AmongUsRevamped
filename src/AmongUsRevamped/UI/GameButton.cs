using System;
using System.Collections.Generic;
using System.ComponentModel;
using AmongUsRevamped.Events;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using InnerNet;
using UnityEngine;
using ButtonManager = KillButtonManager;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.UI
{
    public partial class GameButton : IDisposable
    {
        protected static List<GameButton> Buttons = new();

        public static float ButtonSize = 1.3f;

        /// <summary>
        /// Game Hud state
        /// </summary>
        public static bool HudVisible { get; private set; } = true;

        /// <summary>
        /// Get whether the game is active
        /// </summary>
        protected static bool GameActive
        {
            get
            {
                return GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance &&
                    (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay);
            }
        }

        /// <summary>
        /// Get whether the player can click
        /// </summary>
        protected static bool PlayerCanClick
        {
            get
            {
                return PlayerControl.LocalPlayer?.CanMove == true;
            }
        }

        static GameButton()
        {
            HudEvents.HudStateChanged += (sender, e) => HudVisible = e.Active;
        }

        public const float EdgeOffsetX = 0.8F;
        public const float EdgeOffsetY = 0.7F;
        public HudPosition Position { get; set; }

        public KeyCode HotKey { get; set; } = KeyCode.None;

        /// <summary>
        /// Whether the button is displayed.
        /// </summary>
        /// <remarks>
        /// Non visible button cannot be clicked.
        /// </remarks>
        public virtual bool Visible { get; set; } = true;
        /// <summary>
        /// Whether the button is clickable.
        /// </summary>
        public virtual bool Clickable { get; set; } = true;
        /// <summary>
        /// Whether the button is usable.
        /// </summary>
        public virtual bool IsUsable => HudVisible && Visible && Clickable && GameActive;
        public virtual bool Exists => GameActive && PlayerControl.LocalPlayer?.Data != null && ButtonManager;

        protected Sprite Sprite;

        /// <summary>
        /// Raised on hud update
        /// </summary>
        public event EventHandler<EventArgs> OnUpdate;
        /// <summary>
        /// Raised when the button is clicked (when Visible and Clickable are both true).
        /// <para>Cancellable</para>
        /// </summary>
        public event EventHandler<CancelEventArgs> OnClick;
        /// <summary>
        /// Raised after button click, if not cancelled by <see cref="OnClick"/>.
        /// </summary>
        public event EventHandler<EventArgs> Clicked;

        public int Index { get; private set; }
        protected ButtonManager ButtonManager;
        protected bool Disposed;

        public GameButton(Sprite sprite, HudPosition position)
        {
            Index = Buttons.Count;

            Position = position ?? throw new ArgumentNullException(nameof(position), "Position cannot be null.");
            Position.Offset += new Vector2(EdgeOffsetX, EdgeOffsetY);

            Buttons.Add(this);

            if (sprite) UpdateSprite(sprite);

            CreateButton();
            HudEvents.HudUpdated += HudUpdate;
        }

        public GameButton(byte[] imageData, HudPosition position) :
            this(AssetUtils.LoadSpriteFromBytes(imageData ?? throw new ArgumentNullException(nameof(imageData), "Image data required.")), position)
        {
        }

        public GameButton(string imageResourcePath, HudPosition position) :
            this(AssetUtils.LoadSpriteFromResource(imageResourcePath), position)
        {
        }

        protected virtual void HudUpdate(object sender, EventArgs e)
        {
            if (Disposed) // Button disposed, remove events and reference
            {
                HudEvents.HudUpdated -= HudUpdate;
                Buttons.Remove(this);
                return;
            }

            try
            {
                CreateButton();
                Update();
            }
            catch (Exception ex)
            {
                AmongUsRevamped.LogWarning($"An exception has occurred updating a button: {ex}");
            }
        }

        public virtual void UpdateSprite(Sprite sprite)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite), $"Sprite image required.");

            try
            {
                Sprite?.texture?.Destroy();
                Sprite?.Destroy();
            }
            catch
            {
            }

            Sprite = Object.Instantiate(sprite).DontDestroy();
            Sprite.texture.DontDestroy();
        }

        protected virtual void CreateButton()
        {
            // Button already created or no KillButton component available
            if (ButtonManager || !HudManager.Instance?.KillButton) return;

            // Instantiate button game object
            ButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
            ButtonManager.name = $"{GetType().Name}_{Index}";
            ButtonManager.TimerText.enabled = false;
            ButtonManager.TimerText.gameObject.SetActive(false);

            SetClickable(Clickable);
            SetVisible(HudVisible && Visible);

            if (Sprite) ButtonManager.renderer.sprite = Sprite;

            PassiveButton button = ButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(() => PerformClick()));
        }

        protected virtual void Update()
        {
            if (Disposed) return;

            if (!Exists)
            {
                if (ButtonManager)
                {
                    SetClickable(false);
                    SetVisible(false);
                }

                return;
            }

            if (Sprite) ButtonManager.renderer.sprite = Sprite;
            RaiseOnUpdate();

            if (Disposed) return;

            SetClickable(PlayerCanClick && Clickable);
            SetVisible(HudVisible && Visible);

            if (HotKey != KeyCode.None && Input.GetKeyDown(HotKey)) PerformClick();
        }

        protected virtual void SetVisible(bool visible)
        {
            if (visible) UpdatePosition();

            ButtonManager.gameObject.SetActive(visible);
            ButtonManager.renderer.enabled = visible;
        }

        protected virtual void SetClickable(bool active)
        {
            ButtonManager.renderer.color = !active ? Palette.DisabledClear : Palette.EnabledColor;
            ButtonManager.renderer.material.SetFloat("_Desat", active ? 0f : 1f);
        }

        protected virtual void UpdatePosition()
        {
            ButtonManager.transform.localPosition = Position.GetVector3(-9f);
        }

        /// <summary>
        /// Performs button click.
        /// </summary>
        /// <returns>Click success.</returns>
        public virtual bool PerformClick()
        {
            // Button not usable or click was cancelled.
            if (!IsUsable || !RaiseOnClick()) return false;

            RaiseClicked();

            return true;
        }

        /// <summary>
        /// Raises <see cref="OnClick"/> event.
        /// </summary>
        /// <returns>False if click was cancelled.</returns>
        protected bool RaiseOnClick()
        {
            CancelEventArgs args = new();
            OnClick?.SafeInvoke(this, args, nameof(OnClick));
            return !args.Cancel;
        }

        /// <summary>
        /// Raises <see cref="Clicked"/> event.
        /// </summary>
        protected void RaiseClicked()
        {
            Clicked?.SafeInvoke(this, EventArgs.Empty, nameof(Clicked));
        }

        /// <summary>
        /// Raises <see cref="OnUpdate"/> event.
        /// </summary>
        protected void RaiseOnUpdate()
        {
            OnUpdate?.SafeInvoke(this, EventArgs.Empty, nameof(OnUpdate));
        }

        /// <summary>
        /// Clean up
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    if (ButtonManager)
                    {
                        SetClickable(false);
                        SetVisible(false);
                        ButtonManager.Destroy();
                    }

                    HudEvents.HudUpdated -= HudUpdate;
                    Buttons.Remove(this);

                    Sprite?.texture?.Destroy();
                    Sprite?.Destroy();
                }
                catch
                {
                }
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
}
