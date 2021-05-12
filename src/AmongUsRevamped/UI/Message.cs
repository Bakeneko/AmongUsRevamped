using System;
using System.Collections;
using System.Collections.Generic;
using AmongUsRevamped.Events;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.UI
{
    public class Message : IDisposable
    {
        protected static List<Message> Messages = new();

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

        static Message()
        {
            HudEvents.HudStateChanged += (sender, e) => HudVisible = e.Active;
        }

        public HudPosition Position { get; set; } = new HudPosition(0f, 1f, HudAlignment.Bottom);

        /// <summary>
        /// Whether the message is displayed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public virtual bool Visible { get; set; } = true;

        /// <summary>
        /// Message text.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// Message tint color.
        /// </summary>
        public virtual Color Color { get; set; } = Color.white;

        public virtual bool Exists => GameActive && PlayerControl.LocalPlayer?.Data != null && MessageObject;

        public SpriteRenderer Renderer;
        protected Sprite Sprite;

        public TMP_Text TextArea;

        /// <summary>
        /// Raised on hud update
        /// </summary>
        public event EventHandler<EventArgs> OnUpdate;

        public int Index { get; private set; }
        protected GameObject MessageObject;
        protected bool Disposed;

        public Message(float duration, string text, Color color = default, Sprite sprite = null)
        {
            Index = Messages.Count;
            Messages.Add(this);

            Color = color != default ? color : Color.white;
            Text = text;

            if (sprite == null) sprite = AssetUtils.LoadSpriteFromResource("AmongUsRevamped.Resources.Sprites.message_bubble.png");
            
            UpdateSprite(sprite);

            CreateMessage();
            HudEvents.HudUpdated += HudUpdate;

            if (duration > 0) MakeDisappear(duration);
        }

        public void MakeDisappear(float delay = 0)
        {
            if (delay == 0)
            {
                Dispose();
                return;
            }
            Coroutines.Start(DisappearCoroutine(this, delay));
        }

        protected virtual void HudUpdate(object sender, EventArgs e)
        {
            if (Disposed) // Message disposed, remove events and reference
            {
                HudEvents.HudUpdated -= HudUpdate;
                Messages.Remove(this);
                return;
            }

            try
            {
                CreateMessage();
                Update();
            }
            catch (Exception ex)
            {
                AmongUsRevamped.LogWarning($"An exception has occurred updating a message: {ex}");
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

        protected virtual void CreateMessage()
        {
            // Message already created or no roomTracker object available
            if (MessageObject || !(HudManager.Instance?.roomTracker ?? false)) return;

            // Instantiate message game objects
            MessageObject = new($"{GetType().Name}_{Index}");
            MessageObject.layer = 5;
            MessageObject.transform.parent = HudManager.Instance.transform;

            Renderer = MessageObject.AddComponent<SpriteRenderer>();
            if (Sprite) Renderer.sprite = Sprite;

            var text = Object.Instantiate(HudManager.Instance.roomTracker.gameObject, MessageObject.transform);
            text.name = "MessageText";
            text.GetComponent<RoomTracker>()?.DestroyImmediate();
            TextArea = text.GetComponent<TMP_Text>();
            TextArea.rectTransform.anchoredPosition3D = Vector3.zero;
            TextArea.rectTransform.offsetMax = new Vector2(2.15f, 0.2f);
            TextArea.rectTransform.offsetMin = new Vector2(-2.15f, -0.2f);
            TextArea.name = "MessageText";
            TextArea.color = Color.black;
            TextArea.SetOutlineThickness(0f);
            TextArea.fontSizeMax = 2f;
        }

        protected virtual void Update()
        {
            if (Disposed) return;

            if (!Exists)
            {
                if (MessageObject)
                {
                    SetVisible(false);
                }

                return;
            }

            TextArea.text = Text;
            Renderer.color = Color;
            if (Sprite) Renderer.sprite = Sprite;

            UpdatePosition();

            RaiseOnUpdate();

            if (Disposed) return;

            SetVisible(HudVisible && Visible);
        }

        protected virtual void UpdatePosition()
        {
            MessageObject.transform.localPosition = Position.GetVector3(1);
        }

        protected virtual void SetVisible(bool visible)
        {
            MessageObject.SetActive(visible);
            Renderer.enabled = visible;
            TextArea.enabled = visible;
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
                    if (MessageObject)
                    {
                        SetVisible(false);
                        MessageObject.Destroy();
                    }

                    HudEvents.HudUpdated -= HudUpdate;
                    Messages.Remove(this);

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

        protected internal static IEnumerator DisappearCoroutine(Message message, float duration)
        {
            yield return new WaitForSeconds(duration);
            message?.Dispose();
        }
    }
}
