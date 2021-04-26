using BepInEx.Configuration;

namespace AmongUsRevamped.Options
{
    public interface IToggleOption
    {
        public void Toggle();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling toggle options.
    /// </summary>
    public class CustomToggleOption : CustomOption, IToggleOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.Persist"/> is false.
        /// </remarks>
        public readonly ConfigEntry<bool> ConfigEntry;

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        /// <param name="indent">Option indented in the Hud</param>
        public CustomToggleOption(string id, string name, bool persist, bool value, bool indent = false) : base(id, name, persist, CustomOptionType.Toggle, value)
        {
            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is ToggleOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = persist ? AmongUsRevamped.Instance.Config.Bind(SectionId, ConfigId, GetDefaultValue(), name) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = (sender, value) => ((bool)value) ? "On" : "Off";
            Indent = indent;
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionValueChangedEventArgs(value, Value);
        }

        /// <summary>
        /// Toggles the option value.
        /// </summary>
        public virtual void Toggle()
        {
            SetValue(!GetValue());
        }

        protected virtual void SetValue(bool newValue, bool raiseEvents)
        {
            base.SetValue(newValue, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="newValue">The new value</param>
        public virtual void SetValue(bool newValue)
        {
            SetValue(newValue, true);
        }

        /// <returns>The boolean-casted default value.</returns>
        public virtual bool GetDefaultValue()
        {
            return GetDefaultValue<bool>();
        }

        /// <returns>The boolean-casted old value.</returns>
        public virtual bool GetOldValue()
        {
            return GetOldValue<bool>();
        }

        /// <returns>The boolean-casted current value.</returns>
        public virtual bool GetValue()
        {
            return GetValue<bool>();
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        /// <param name="indent">Option indented in the Hud</param>
        public static CustomToggleOption AddToggle(string id, string name, bool persist, bool value, bool indent = false)
        {
            return new CustomToggleOption(id, name, persist, value, indent);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="value">Default value</param>
        public static CustomToggleOption AddToggle(string id, string name, bool value)
        {
            return AddToggle(id, name, true, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="value">Default value</param>
        public static CustomToggleOption AddToggle(string name, bool persist, bool value)
        {
            return AddToggle(name, name, persist, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="name">Name/title of the option</param>
        /// <param name="value">Default value</param>
        public static CustomToggleOption AddToggle(string name, bool value)
        {
            return AddToggle(name, name, value);
        }
    }
}
