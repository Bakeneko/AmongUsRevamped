using System;
using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.UI
{
    public partial class CooldownButton : GameButton
    {
        protected static List<CooldownButton> CooldownButtons = new();

        protected float _initialCDDuration = 0f;
        /// <summary>
        /// Button initial cd duration. Set to 0 for no cd.
        /// </summary>
        /// <remarks>Base kill button use 10s.</remarks>
        public virtual float InitialCooldownDuration { get { return _initialCDDuration; } set { _initialCDDuration = Mathf.Max(0f, value); } }

        protected float _cooldownDuration = 0f;
        /// <summary>
        /// Button cd duration. Set to 0 for no cd.
        /// </summary>
        public virtual float CooldownDuration { get { return _cooldownDuration; } set { _cooldownDuration = Mathf.Max(0f, value); } }

        protected float _cooldownTime = 0f;
        /// <summary>
        /// Remaing button cooldown time.
        /// </summary>
        public virtual float CooldownTime { get { return _cooldownTime; } protected set { _cooldownTime = Mathf.Max(0f, value); ; } }

        /// <summary>
        /// Whether or not <see cref="CooldownTime"/> is larger than 0.
        /// </summary>
        public virtual bool IsCoolingDown { get { return CooldownTime > 0f; } }

        protected float _effectDuration = 0F;
        /// <summary>
        /// Duration of the effect applied on click.
        /// </summary>
        public virtual float EffectDuration { get { return _effectDuration; } set { _effectDuration = Mathf.Max(0f, value); } }

        /// <summary>
        /// Whether or not <see cref="EffectDuration"/> is larger than 0.
        /// </summary>
        public virtual bool HasEffect { get { return EffectDuration > 0f; } }

        protected float _effectTime = 0f;
        /// <summary>
        /// Remaing button effect time.
        /// </summary>
        public virtual float EffectTime { get { return _effectTime; } protected set { _effectTime = Mathf.Max(0f, value); ; } }

        /// <summary>
        /// Whether or not <see cref="EffectTime"/> is larger than 0.
        /// </summary>
        public virtual bool IsEffectActive { get { return EffectTime > 0f; } }

        /// <summary>
        /// Whether the remaining effect duration will only decrease when the cooldown can decrease (as opposed to always decrease when set to false).
        /// <para>See <see cref="CanUpdateCooldown"/> for when the cooldown can decrease.</para>
        /// </summary>
        public virtual bool EffectCanPause { get; set; } = false;

        /// <summary>
        /// Whether the remaining cooldown duration should decrease.
        /// </summary>
        public virtual bool CanUpdateCooldown { get { return Visible && !IntroCutscene.Instance && !MeetingHud.Instance && !ExileController.Instance; } }

        /// <summary>
        /// Whether the effect will end early when a meeting is called.
        /// </summary>
        public virtual bool MeetingsEndEffect { get; set; } = true;

        /// <summary>
        /// Whether cooldown will be applied after meetings.
        /// </summary>
        public virtual bool CooldownAfterMeetings { get; set; } = true;

        /// <summary>
        /// Whether the button is displayed.
        /// </summary>
        /// <remarks>
        /// Non visible button cannot be clicked.
        /// <para>Non visible button cd does not decrease but effect duration does.</para>
        /// </remarks>
        public override bool Visible { get; set; } = true;

        /// <summary>
        /// Whether the button is usable.
        /// </summary>
        public override bool IsUsable { get { return !IsCoolingDown && base.IsUsable; } }

        public Color EffectColor { get; set; } = new Color(0f, 0.8f, 0f);

        /// <summary>
        /// Raised when the effect duration starts, when <see cref="HasEffect"/> is true and after the button has been clicked and the click hasn't been cancelled.
        /// </summary>
        public virtual event EventHandler<EventArgs> EffectStarted;
        /// <summary>
        /// Raised after the effect ends (due to duration or meeting).
        /// </summary>
        public virtual event EventHandler<EventArgs> EffectEnded;
        /// <summary>
        /// Raised when the cooldown starts, either after the button has been clicked (click not cancelled), or after the effect duration if <see cref="HasEffect"/> is true.
        /// </summary>
        public virtual event EventHandler<EventArgs> CooldownStarted;
        /// <summary>
        /// Raised after the cooldown ends.
        /// </summary>
        public virtual event EventHandler<EventArgs> CooldownEnded;

        public CooldownButton(Sprite sprite, HudPosition position, float cooldownDuration, float effectDuration = 0f, float initialCooldown = 0f) :
            base(sprite, position)
        {
            Init(cooldownDuration, effectDuration, initialCooldown);
        }

        public CooldownButton(byte[] imageData, HudPosition position, float cooldownDuration, float effectDuration = 0f, float initialCooldown = 0f) :
            base(imageData, position)
        {
            Init(cooldownDuration, effectDuration, initialCooldown);
        }

        public CooldownButton(string imageResourcePath, HudPosition position, float cooldownDuration, float effectDuration = 0f, float initialCooldown = 0f) :
            base(imageResourcePath, position)
        {
            Init(cooldownDuration, effectDuration, initialCooldown);
        }

        private void Init(float cooldownDuration, float effectDuration = 0F, float initialCooldown = 0F)
        {
            CooldownDuration = cooldownDuration;
            EffectDuration = effectDuration;
            InitialCooldownDuration = initialCooldown;
            CooldownTime = InitialCooldownDuration;

            CooldownButtons.Add(this);
        }

        public override bool PerformClick()
        {
            if (!base.PerformClick()) return false;

            if (HasEffect) StartEffect();
            else ApplyCooldown();

            return true;
        }

        protected override void Update()
        {
            if (Disposed) return;

            if (!Exists)
            {
                EndEffect(false, false);

                if (ButtonManager)
                {
                    SetClickable(false);
                    SetVisible(false);
                }

                return;
            }

            if (Sprite) ButtonManager.renderer.sprite = Sprite;

            UpdateCooldown();

            RaiseOnUpdate();

            if (Disposed) return;

            SetClickable(PlayerCanClick && Clickable);
            SetVisible(HudVisible && Visible);

            if (HotKey != KeyCode.None && Input.GetKeyDown(HotKey)) PerformClick();
        }

        /// <summary>
        /// Starts the effect duration and raises the <see cref="EffectStarted"/> event.
        /// </summary>
        public virtual void StartEffect()
        {
            bool wasEffectActive = IsEffectActive;
            EffectTime = CooldownTime = EffectDuration;
            ButtonManager.TimerText.color = EffectColor;

            if (!wasEffectActive) RaiseEffectStarted();
        }

        /// <summary>
        /// Ends the effect duration and raises the <see cref="EffectEnd"/> event.
        /// </summary>
        /// <param name="startCooldown">Whether or not to start the cooldown</param>
        public virtual void EndEffect(bool wasActive, bool startCooldown = true)
        {
            EffectTime = 0;
            ButtonManager.TimerText.color = Palette.EnabledColor;
            if (wasActive)
            {
                RaiseEffectEnded();
            }

            if (startCooldown) ApplyCooldown();
        }

        /// <summary>
        /// Updates the button remaining cooldown or effect duration.
        /// </summary>
        protected virtual void UpdateCooldown()
        {
            bool effectActive = IsEffectActive;
            if (!IsCoolingDown || (!effectActive || EffectCanPause) && !CanUpdateCooldown) return;

            CooldownTime -= Time.deltaTime;
            EffectTime -= Time.deltaTime;

            if (effectActive && !IsEffectActive) EndEffect(true);

            if (!IsCoolingDown) RaiseCooldownEnded();
        }

        protected override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            UpdateCooldownVisuals(visible);
        }

        /// <summary>
        /// Sets the button on cooldown. Defaults to <see cref="CooldownDuration"/>.
        /// </summary>
        /// <remarks>Raises the <see cref="CooldownStarted"/> or <see cref="CooldownEnded"/> events depending on whether the button was on cooldown or not.</remarks>
        /// <remarks>Ends effect duration if the effect is active when called.</remarks>
        /// <param name="customCooldown">Optional custom cooldown duration (does not affect <see cref="CooldownDuration"/>, may be longer or shorter than <see cref="CooldownDuration"/>)</param>
        public virtual void ApplyCooldown(float? customCooldown = null)
        {
            EndEffect(IsEffectActive, false);

            bool wasCoolingDown = IsCoolingDown;

            CooldownTime = customCooldown ?? CooldownDuration;

            if (!wasCoolingDown && IsCoolingDown)
            {
                RaiseCooldownStarted();
            }
            else if (wasCoolingDown && !IsCoolingDown)
            {
                RaiseCooldownEnded();
            }
        }

        /// <summary>
        /// Updates the button cooldown text and progress visual.
        /// </summary>
        protected virtual void UpdateCooldownVisuals(bool visible)
        {
            bool timerActive = visible && (IsCoolingDown || IsEffectActive);

            if (visible)
            {
                float cooldownRate = CooldownDuration == 0f ? IsCoolingDown ? 1f : 0f :
                    Mathf.Clamp(CooldownTime / (IsEffectActive ? EffectDuration : CooldownDuration), 0f, 1f);
                ButtonManager.renderer?.material?.SetFloat("_Percent", cooldownRate);

                ButtonManager.TimerText.text = Mathf.CeilToInt(CooldownTime).ToString();
            }

            ButtonManager.TimerText.enabled = timerActive;
            ButtonManager.TimerText.gameObject.SetActive(timerActive);
        }

        /// <summary>
        /// Raises <see cref="EffectStarted"/> event.
        /// </summary>
        protected void RaiseEffectStarted()
        {
            EffectStarted?.SafeInvoke(this, EventArgs.Empty, nameof(EffectStarted));
        }

        /// <summary>
        /// Raises <see cref="EffectEnded"/> event.
        /// </summary
        protected void RaiseEffectEnded()
        {
            EffectEnded?.SafeInvoke(this, EventArgs.Empty, nameof(EffectEnded));
        }

        /// <summary>
        /// Raises <see cref="CooldownStarted"/> event.
        /// </summary>
        protected void RaiseCooldownStarted()
        {
            CooldownStarted?.SafeInvoke(this, EventArgs.Empty, nameof(CooldownStarted));
        }

        /// <summary>
        /// Raises <see cref="CooldownEnded"/> event.
        /// </summary>
        protected void RaiseCooldownEnded()
        {
            CooldownEnded?.SafeInvoke(this, EventArgs.Empty, nameof(CooldownEnded));
        }
    }
}
