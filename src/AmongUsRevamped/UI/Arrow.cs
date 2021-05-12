using System;
using System.Collections.Generic;
using AmongUsRevamped.Events;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using InnerNet;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.UI
{
    public class Arrow : IDisposable
    {
        protected static List<Arrow> Arrows = new();

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

        static Arrow()
        {
            HudEvents.HudStateChanged += (sender, e) => HudVisible = e.Active;
        }

        /// <summary>
        /// Whether the arrow is displayed.
        /// </summary>
        public virtual bool Visible { get; set; } = true;

        /// <summary>
        /// Arrow tint color.
        /// </summary>
        public virtual Color Color { get; set; } = Color.white;

        public virtual Vector3 Target { get; set; } = Vector3.zero;
        protected Vector3 PreviousTarget = Vector3.zero;

        public virtual float Percent { get; set; } = 0.925f;

        public virtual bool Exists => GameActive && PlayerControl.LocalPlayer?.Data != null && ArrowObject;

        public SpriteRenderer Renderer;
        protected Sprite Sprite;

        /// <summary>
        /// Raised on hud update
        /// </summary>
        public event EventHandler<EventArgs> OnUpdate;

        public int Index { get; private set; }
        protected GameObject ArrowObject;
        protected bool Disposed;

        public Arrow(Sprite sprite, Color color = default)
        {
            Index = Arrows.Count;
            Arrows.Add(this);

            Color = color != default ? color : Color.white;

            if (sprite == null) sprite = AssetUtils.LoadSpriteFromResource("AmongUsRevamped.Resources.Sprites.arrow.png");

            UpdateSprite(sprite);

            CreateArrow();
            HudEvents.HudUpdated += HudUpdate;
        }

        public Arrow(byte[] imageData, Color color = default) :
            this(AssetUtils.LoadSpriteFromBytes(imageData ?? throw new ArgumentNullException(nameof(imageData), "Image data required.")), color)
        {
        }

        public Arrow(string imageResourcePath, Color color = default) :
            this(AssetUtils.LoadSpriteFromResource(imageResourcePath), color)
        {
        }

        protected virtual void HudUpdate(object sender, EventArgs e)
        {
            if (Disposed) // Arrow disposed, remove events and reference
            {
                HudEvents.HudUpdated -= HudUpdate;
                Arrows.Remove(this);
                return;
            }

            try
            {
                CreateArrow();
                Update();
            }
            catch (Exception ex)
            {
                AmongUsRevamped.LogWarning($"An exception has occurred updating an arrow: {ex}");
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

        protected virtual void CreateArrow()
        {
            // Arrow already created
            if (ArrowObject) return;

            // Instantiate arrow game objects
            ArrowObject = new($"{GetType().Name}_{Index}");
            ArrowObject.layer = 5;
            ArrowObject.transform.parent = PlayerControl.LocalPlayer.gameObject.transform;

            Renderer = ArrowObject.AddComponent<SpriteRenderer>();
            Renderer.sprite = Sprite;
        }

        protected virtual void Update()
        {
            if (Disposed) return;

            if (!Exists)
            {
                if (ArrowObject)
                {
                    SetVisible(false);
                }

                return;
            }

            if (Target == null) Target = PreviousTarget;
            if (Target == null) Target = Vector3.zero;

            // Process target
            Camera main = Camera.main;
            Vector2 vector = Target - main.transform.position;
            float perc = vector.magnitude / (main.orthographicSize * Percent);
            // Don't display arrow if too close to target
            bool shouldDisplay = perc > 0.3d;
            Vector2 vector2 = main.WorldToViewportPoint(Target);

            if (Between(vector2.x, 0f, 1f) && Between(vector2.y, 0f, 1f))
            {
                ArrowObject.transform.position = Target - (Vector3)vector.normalized * 0.6f;
                float scale = Mathf.Clamp(perc, 0f, 1f);
                ArrowObject.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                Vector2 vector3 = new(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
                float orthographicSize = main.orthographicSize;
                float num3 = main.orthographicSize * main.aspect;
                Vector3 vector4 = new(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
                ArrowObject.transform.position = main.transform.position + vector4;
                ArrowObject.transform.localScale = Vector3.one;
            }

            LookAt2D(ArrowObject.transform, Target);

            Renderer.color = Color;
            if (Sprite) Renderer.sprite = Sprite;

            RaiseOnUpdate();

            if (Disposed) return;

            SetVisible(HudVisible && Visible && shouldDisplay);
        }

        protected void LookAt2D(Transform transform, Vector3 target)
        {
            Vector3 vector = target - transform.position;
            vector.Normalize();
            float distance = Mathf.Atan2(vector.y, vector.x);
            if (transform.lossyScale.x < 0f) distance += 3.1415927f;
            transform.rotation = Quaternion.Euler(0f, 0f, distance * 57.29578f);
        }

        protected bool Between(float value, float min, float max)
        {
            return value > min && value < max;
        }

        protected virtual void SetVisible(bool visible)
        {
            ArrowObject.SetActive(visible);
            Renderer.enabled = visible;
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
                    if (ArrowObject)
                    {
                        SetVisible(false);
                        ArrowObject.Destroy();
                    }

                    HudEvents.HudUpdated -= HudUpdate;
                    Arrows.Remove(this);

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
