using BepInEx.Configuration;
using System;
using UnityEngine;

namespace AmongUsRevamped.Options
{

    public interface INumberOption
    {
        public void Increase();
        public void Decrease();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling number options.
    /// </summary>
    public class CustomNumberOption : CustomOption, INumberOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.Persist"/> is false.
        /// </remarks>
        public readonly ConfigEntry<float> ConfigEntry;

        /// <summary>
        /// A "modifier" string format.
        /// </summary>
        public static Func<CustomOption, object, string> ModifierStringFormat { get; } = (sender, value) => $"{value}x";

        /// <summary>
        /// A "seconds" string format.
        /// </summary>
        public static Func<CustomOption, object, string> SecondsStringFormat { get; } = (sender, value) => $"{value:0.0#}s";

        /// <summary>
        /// A "percent" string format.
        /// </summary>
        public static Func<CustomOption, object, string> PercentStringFormat { get; } = (sender, value) => $"{value:0}%";

        /// <summary>
        /// The lowest permitted value.
        /// </summary>
        public readonly float Min;

        /// <summary>
        /// The highest permitted value.
        /// </summary>
        public readonly float Max;

        /// <summary>
        /// The increment or decrement steps when <see cref="Increase"/> or <see cref="Decrease"/> are called.
        /// </summary>
        public readonly float Increment;

        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        /// <param name="min">Lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">Highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">Increment or decrement steps when <see cref="Increase"/> or <see cref="Decrease"/> are called</param>
        /// <param name="indent">Option indented in the Hud</param>
        /// <param name="valueFormat">String format used to display value</param>
        public CustomNumberOption(string id, string name, bool persist, float value, float min = 0.25F, float max = 5F, float increment = 0.25F, bool indent = false, Func<CustomOption, object, string> valueFormat = null) : base(id, name, persist, CustomOptionType.Number, value)
        {
            Min = Mathf.Min(value, min);
            Max = Mathf.Max(value, max);

            Increment = increment;

            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is NumberOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };


            ConfigEntry = persist ? AmongUsRevamped.Instance.Config.Bind(SectionId, ConfigId, GetDefaultValue(), name) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = valueFormat ?? ((sender, value) => value.ToString());
            Indent = indent;
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object newValue, object oldValue)
        {
            return new NumberOptionOnValueChangedEventArgs(newValue, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object newValue, object oldValue)
        {
            return new NumberOptionValueChangedEventArgs(newValue, Value);
        }

        protected override bool GameObjectCreated(OptionBehaviour obj)
        {
            if (obj is not NumberOption number) return false;

            number.ValidRange = new FloatRange(Min, Max);
            number.Increment = Increment;

            return UpdateGameObject();
        }

        /// <summary>
        /// Increases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's lower or until it matches <see cref="Max"/>.
        /// </summary>
        public virtual void Increase()
        {
            SetValue(GetValue() + Increment);
        }

        /// <summary>
        /// Decreases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's higher or until it matches <see cref="Min"/>.
        /// </summary>
        public virtual void Decrease()
        {
            SetValue(GetValue() - Increment);
        }

        protected virtual void SetValue(float newValue, bool raiseEvents)
        {
            newValue = Mathf.Clamp(newValue, Min, Max);
            base.SetValue(newValue, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="newValue">The new value</param>
        public virtual void SetValue(float newValue)
        {
            SetValue(newValue, true);
        }

        /// <returns>The float-casted default value.</returns>
        public virtual float GetDefaultValue()
        {
            return GetDefaultValue<float>();
        }

        /// <returns>The float-casted old value.</returns>
        public virtual float GetOldValue()
        {
            return GetOldValue<float>();
        }

        /// <returns>The float-casted current value.</returns>
        public virtual float GetValue()
        {
            return GetValue<float>();
        }
    }

    public partial class CustomOption
    {

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        /// <param name="min">Lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">Highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">Increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        /// <param name="indent">Option indented in the Hud</param>
        /// <param name="valueFormat">String format used to display value</param>
        public static CustomNumberOption AddNumber(string id, string name, bool persist, float value, float min = 0.25F, float max = 5F, float increment = 0.25F, bool indent = false, Func<CustomOption, object, string> valueFormat = null)
        {
            return new CustomNumberOption(id, name, persist, value, min, max, increment, indent, valueFormat);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="id">Id of the option, used to persist value and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="value">Default value</param>
        /// <param name="min">Lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">Highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">Increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        /// <param name="indent">Option indented in the Hud</param>
        /// <param name="valueFormat">String format used to display value</param>
        public static CustomNumberOption AddNumber(string id, string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F, bool indent = false, Func<CustomOption, object, string> valueFormat = null)
        {
            return AddNumber(id, name, true, value, min, max, increment, indent, valueFormat);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        /// <param name="min">Lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">Highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">Increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        /// <param name="indent">Option indented in the Hud</param>
        /// <param name="valueFormat">String format used to display value</param>
        public static CustomNumberOption AddNumber(string name, bool persist, float value, float min = 0.25F, float max = 5F, float increment = 0.25F, bool indent = false, Func<CustomOption, object, string> valueFormat = null)
        {
            return AddNumber(name, name, persist, value, min, max, increment, indent, valueFormat);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="name">Name/title of the option</param>
        /// <param name="value">Default value</param>
        /// <param name="min">Lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">Highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">Increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        /// <param name="indent">Option indented in the Hud</param>
        /// <param name="valueFormat">String format used to display value</param>
        public static CustomNumberOption AddNumber(string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F, bool indent = false, Func<CustomOption, object, string> valueFormat = null)
        {
            return AddNumber(name, true, value, min, max, increment, indent, valueFormat);
        }
    }
}
