using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Utils;
using AmongUsRevamped.Extensions;
using TMPro;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Options
{
    public enum CustomOptionType : byte
    {
        /// <summary>
        /// A checkmark toggle option.
        /// </summary>
        Toggle,
        /// <summary>
        /// A float number option with increase/decrease buttons.
        /// </summary>
        Number,
        /// <summary>
        /// A string option (underlying int) with forward/back arrows.
        /// </summary>
        String,
        /// <summary>
        /// A button option.
        /// </summary>
        Button
    }

    /// <summary>
    /// A class wrapping all the logic to add custom lobby options.
    /// </summary>
    public partial class CustomOption
    {
        /// <summary>
        /// List of added options.
        /// </summary>
        public static List<CustomOption> Options = new List<CustomOption>();

        /// <summary>
        /// Enable/disable debug logging messages.
        /// </summary>
        public static bool Debug { get; set; } = false;

        /// <summary>
        /// Size of lobby options text, game default is 2, we set it to 1.0F.
        /// </summary>
        public static float HudTextFontSize { get; set; } = 1.0F;

        /// <summary>
        /// Enable/disable the lobby options text scroller.
        /// </summary>
        public static bool Scrollable { get; set; } = true;

        /// <summary>
        /// Clear the game's default options list before listing custom options in the lobby.
        /// </summary>
        public static bool ClearDefaultHudText { get; set; } = false;

        /// <summary>
        /// ID of the plugin that created the option.
        /// </summary>
        public readonly string PluginId;

        /// <summary>
        /// Key value used in the config to store the option's value (when Persist is true).
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Name">Name</see> when unspecified.
        /// </remarks>
        public readonly string ConfigId;

        /// <summary>
        /// Used to sync the value of the option between players.
        /// </summary>
        /// <remarks>
        /// Combines <see cref="PluginId">PluginId</see> and <see cref="ConfigId">ConfigId</see> with an underscore between.
        /// </remarks>
        public readonly string Id;

        /// <summary>
        /// Name/title of the option.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Specifies whether to persist option's value (only applies for the lobby host).
        /// </summary>
        public readonly bool Persist;

        /// <summary>
        /// Option type.
        /// See <see cref="CustomOptionType"/>.
        /// </summary>
        public readonly CustomOptionType Type;

        /// <summary>
        /// Default value.
        /// </summary>
        public readonly object DefaultValue;

        /// <summary>
        /// Previous value, may match <see cref="Value">Value</see> when it matches <see cref="DefaultValue">DefaultValue</see>
        /// </summary>
        protected virtual object OldValue { get; set; }

        /// <summary>
        /// Current value of the option.
        /// </summary>
        protected virtual object Value { get; set; }

        protected readonly byte[] Hash;

        /// <summary>
        /// An event raised before a value change occurs, can alter the final value or cancel the value change. Only raised for the lobby host.
        /// See <see cref="OptionOnValueChangedEventArgs"/> and childs <seealso cref="ToggleOptionOnValueChangedEventArgs"/>, <seealso cref="NumberOptionOnValueChangedEventArgs"/> and <seealso cref="StringOptionOnValueChangedEventArgs"/>.
        /// </summary>
        public event EventHandler<OptionOnValueChangedEventArgs> OnValueChanged;

        /// <summary>
        /// An event raised after the option's value has changed.
        /// See <see cref="OptionValueChangedEventArgs"/> and childs <seealso cref="ToggleOptionValueChangedEventArgs"/>, <seealso cref="NumberOptionValueChangedEventArgs"/> and<seealso cref="StringOptionValueChangedEventArgs"/>.
        /// </summary>
        public event EventHandler<OptionValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// The game object that represents the custom option in the lobby options menu.
        /// </summary>
        public virtual OptionBehaviour GameObject { get; set; }

        public static Func<CustomOption, string, string> DefaultNameStringFormat = (_, name) => name;

        /// <summary>
        /// The string format reflecting the option name, result returned by <see cref="GetFormattedName"/>.
        /// <para>Arguments: the sending custom option, option name.</para>
        /// </summary>
        public virtual Func<CustomOption, string, string> NameStringFormat { get; set; } = DefaultNameStringFormat;

        public static Func<CustomOption, object, string> DefaultValueStringFormat = (_, value) => value.ToString();

        /// <summary>
        /// The string format reflecting the value, result returned by <see cref="GetFormattedValue"/>.
        /// <para>Arguments: the sending custom option, current value.</para>
        /// </summary>
        public virtual Func<CustomOption, object, string> ValueStringFormat { get; set; } = DefaultValueStringFormat;
        
        public static Func<CustomOption, string, string, string> DefaultHudStringFormat = (_, name, value) => $"{name}: {value}";

        /// <summary>
        /// The string format reflecting the option name and value, result returned by <see cref="ToString"/>.
        /// Used when displaying the option in the lobby HUD (option list).
        /// <para>Arguments: the sending custom option, formatted name, formatted value.</para>
        /// </summary>
        public virtual Func<CustomOption, string, string, string> HudStringFormat { get; set; } = DefaultHudStringFormat;

        /// <summary>
        /// Option visible in the lobby options menu.
        /// </summary>
        public virtual bool MenuVisible { get; set; } = true;

        /// <summary>
        /// Option visible in the Hud (option list) in the lobby.
        /// </summary>
        public virtual bool HudVisible { get; set; } = true;

        /// <summary>
        /// Option indented in the Hud.
        /// </summary>
        public virtual bool Indent { get; set; } = false;

        /// <summary>
        /// Whether the custom option and it's value changes will be sent through RPC.
        /// </summary>
        public virtual bool SendRpc { get { return true; } }

        /// <param name="id">Id of the option, used to persist value when <paramref name="persist"/> is true and to sync the value between players</param>
        /// <param name="name">Name/title of the option</param>
        /// <param name="persist">Persist option's value (only applies for the lobby host)</param>
        /// <param name="type">Option type. See <see cref="CustomOptionType"/>.</param>
        /// <param name="value">Default value</param>
        protected CustomOption(string id, string name, bool persist, CustomOptionType type, object value)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id), "Option id cannot be null or empty.");

                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Option name cannot be null or empty.");

                if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null");

                PluginId = PluginUtils.GetCallingPluginId();
                ConfigId = id;
                Id = $"{PluginId}_{ConfigId}";

                Name = name;
                Persist = persist;
                Type = type;
                DefaultValue = OldValue = Value = value;

                // Check for existing option
                int i = 0;
                while (Options.Any(option => option.Id.Equals(Id, StringComparison.Ordinal)))
                {
                    Id = $"{Id}_{++i}";
                    ConfigId = $"{id}_{i}";
                }

                Hash = HashUtils.Hash(Id);
                Options.Add(this);
            }
            catch (Exception ex)
            {
                AmongUsRevamped.Logger.LogWarning($"An exception has occurred for option \"{name}\": {ex}");
            }
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionOnValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="oldValue">Current value</param>
        protected virtual OptionOnValueChangedEventArgs OnValueChangedEventArgs(object newValue, object oldValue)
        {
            return new OptionOnValueChangedEventArgs(newValue, Value);
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="newValue">New value</param>
        /// <param name="oldValue">Current value</param>
        protected virtual OptionValueChangedEventArgs ValueChangedEventArgs(object newValue, object oldValue)
        {
            return new OptionValueChangedEventArgs(newValue, Value);
        }

        public bool OnGameObjectCreated(OptionBehaviour obj)
        {
            if (obj == null) return false;

            try
            {
                obj.OnValueChanged = new Action<OptionBehaviour>((_) => { });
                obj.name = obj.gameObject.name = Id;
                GameObject = obj;
                TextMeshPro title = null;

                if (GameObject is ToggleOption toggle) title = toggle.TitleText;
                else if (GameObject is NumberOption number) title = number.TitleText;
                else if (GameObject is StringOption str) title = str.TitleText;
                else if (GameObject is KeyValueOption kv) title = kv.TitleText;

                if (title != null) title.text = GetFormattedName();

                if (!GameObjectCreated(obj))
                {
                    GameObject = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AmongUsRevamped.Logger.LogWarning($"An exception has occurred in {nameof(OnGameObjectCreated)} for option \"{Name}\" ({Type}): {ex}");
            }

            return false;
        }

        /// <summary>
        /// Called when the game object is (re)created for this option.
        /// <para>Depends on <see cref="UpdateGameObject"/> by default.</para>
        /// </summary>
        /// <param name="obj">The game object that was created for this option</param>
        /// <returns>Whether the object is valid, false to destroy.</returns>
        protected virtual bool GameObjectCreated(OptionBehaviour obj)
        {
            return UpdateGameObject();
        }

        /// <summary>
        /// Called when the <see cref="GameObject"/>'s components need to be updated to reflect the value visually.
        /// </summary>
        /// <returns>Update success.</returns>
        protected virtual bool UpdateGameObject()
        {
            try
            {
                if (GameObject is ToggleOption toggle)
                {
                    if (Value is not bool newValue) return false;

                    toggle.oldValue = newValue;
                    if (toggle.CheckMark != null) toggle.CheckMark.enabled = newValue;

                    return true;
                }
                else if (GameObject is NumberOption number)
                {
                    if (Value is float newValue) number.Value = number.oldValue = newValue;

                    if (number.ValueText != null) number.ValueText.text = GetFormattedValue();

                    return true;
                }
                else if (GameObject is StringOption str)
                {
                    if (Value is int newValue) str.Value = str.oldValue = newValue;
                    else if (Value is bool newBoolValue) str.Value = str.oldValue = newBoolValue ? 1 : 0;

                    if (str.ValueText != null) str.ValueText.text = GetFormattedValue();

                    return true;
                }
                else if (GameObject is KeyValueOption kv)
                {
                    if (Value is int newValue) kv.Selected = kv.oldValue = newValue;
                    else if (Value is bool newBoolValue) kv.Selected = kv.oldValue = newBoolValue ? 1 : 0;

                    if (kv.ValueText != null) kv.ValueText.text = GetFormattedValue();

                    return true;
                }
            }
            catch (Exception ex)
            {
                AmongUsRevamped.Logger.LogWarning($"Failed to update game setting value for option \"{Name}\" ({Type}): {ex}");
            }

            return false;
        }

        /// <summary>
        /// Raises a <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="nonDefault">Only raise the event when the current value isn't default</param>
        public void RaiseValueChanged(bool nonDefault = true)
        {
            if (!nonDefault || Value != DefaultValue) ValueChanged?.Invoke(this, ValueChangedEventArgs(Value, DefaultValue));
        }

        /// <summary>
        /// Reset the option to default.
        /// </summary>
        public void SetToDefault(bool raiseEvents = true)
        {
            SetValue(DefaultValue, raiseEvents);
        }

        /// <summary>
        /// Sets the option's value.
        /// </summary>
        /// <remarks>
        /// Does nothing when the value type differs or when the value matches the current value.
        /// </remarks>
        /// <param name="newValue">The new value</param>
        /// <param name="raiseEvents">Whether or not to raise events</param>
        protected virtual void SetValue(object newValue, bool raiseEvents)
        {
            if (newValue?.GetType() != Value?.GetType() || Value == newValue) return; // Refuse value updates that don't match the option type.

            if (raiseEvents && OnValueChanged != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
            {
                object lastValue = newValue;

                OptionOnValueChangedEventArgs args = OnValueChangedEventArgs(newValue, Value);
                foreach (EventHandler<OptionOnValueChangedEventArgs> handler in OnValueChanged.GetInvocationList())
                {
                    handler(this, args);

                    if (args.NewValue.GetType() != newValue.GetType())
                    {
                        args.NewValue = lastValue;
                        args.Cancel = false;

                        AmongUsRevamped.Logger.LogWarning($"A handler for option \"{Name}\" ({Type}) attempted to change value type, ignored.");
                    }

                    lastValue = args.NewValue;

                    if (args.Cancel) return; // Handler cancelled value change.
                }

                newValue = args.NewValue;
            }

            if (OldValue != Value) OldValue = Value;

            Value = newValue;

            if (SendRpc && GameObject != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) Rpc.Instance.Send(this);

            UpdateGameObject();

            if (raiseEvents) ValueChanged?.SafeInvoke(this, ValueChangedEventArgs(newValue, Value), nameof(ValueChanged));

            // Game object does not exist, menu is closed
            if (GameObject == null) return;

            // Refresh the value of all options in the menu, in case an option affects another.
            try
            {
                GameOptionsMenu optionsMenu = Object.FindObjectOfType<GameOptionsMenu>();

                if (optionsMenu == null) return;

                for (int i = 0; i < optionsMenu.Children.Length; i++)
                {
                    OptionBehaviour optionBehaviour = optionsMenu.Children[i];
                    optionBehaviour.enabled = false;
                    optionBehaviour.enabled = true;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the option's value, it's not recommended to call this directly, call derivatives instead.
        /// </summary>
        /// <remarks>
        /// Does nothing when the value type differs or when the value matches the current value.
        /// </remarks>
        /// <param name="newValue">The new value</param>
        public void SetValue(object newValue)
        {
            SetValue(newValue, true);
        }

        /// <summary>
        /// Gets the option's value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted value.</returns>
        public T GetValue<T>()
        {
            return (T)Value;
        }

        /// <summary>
        /// Gets the default option's value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted default value.</returns>
        public T GetDefaultValue<T>()
        {
            return (T)DefaultValue;
        }

        /// <summary>
        /// Gets the old option's value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted old value.</returns>
        public T GetOldValue<T>()
        {
            return (T)OldValue;
        }

        /// <returns><see cref="Name"/> passed through <see cref="NameStringFormat"/>.</returns>
        public string GetFormattedName()
        {
            return (NameStringFormat ?? DefaultNameStringFormat).Invoke(this, Name);
        }

        /// <returns><see cref="Value"/> passed through <see cref="ValueStringFormat"/>.</returns>
        public string GetFormattedValue()
        {
            return (ValueStringFormat ?? DefaultValueStringFormat).Invoke(this, Value);
        }

        /// <returns><see cref="object.ToString()"/> or the return value of <see cref="ValueStringFormat"/> when provided.</returns>
        public override string ToString()
        {
            return (HudStringFormat ?? DefaultHudStringFormat).Invoke(this, GetFormattedName(), GetFormattedValue());
        }
    }
}
