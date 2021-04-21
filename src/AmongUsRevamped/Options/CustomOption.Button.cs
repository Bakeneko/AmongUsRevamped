using System;

namespace AmongUsRevamped.Options
{
    public interface IButtonOption
    {
        public void Click();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling "buttons" in the options menu.
    /// </summary>
    public class CustomButtonOption : CustomOption, IButtonOption
    {
        public override bool SendRpc { get { return false; } }

        public virtual Action ActionOnClick { get; set; }

        /// <summary>
        /// Adds a "button" in the options menu.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        /// <param name="menu">Button will be visible in the lobby options menu</param>
        /// <param name="hud">Button title will appear in the HUD (option list) in the lobby</param>
        /// <param name="actionOnClick">Action to execute on click</param>
        public CustomButtonOption(string id, string title, bool menu = true, bool hud = false, Action actionOnClick = null) : base(id, title, false, CustomOptionType.Button, false)
        {
            HudStringFormat = (_, name, _) => name;
            ValueStringFormat = (_, _) => string.Empty;
            MenuVisible = menu;
            HudVisible = hud;
            ActionOnClick = actionOnClick;
        }

        protected override bool GameObjectCreated(OptionBehaviour obj)
        {
            if (AmongUsClient.Instance?.AmHost != true || obj is not ToggleOption toggle) return false;

            toggle.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
            toggle.transform.FindChild("Background")?.gameObject?.SetActive(true);
            return UpdateGameObject();
        }

        /// <summary>
        /// Toggles the option value (called when the button is pressed).
        /// </summary>
        public virtual void Click()
        {
            ActionOnClick?.Invoke();
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds a "button" in the options menu.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        /// <param name="menu">Button will be visible in the lobby options menu</param>
        /// <param name="hud">Button title will appear in the HUD (option list) in the lobby</param>
        /// <param name="actionOnClick">Action to execute on click</param>
        public static CustomButtonOption AddButton(string id, string title, bool menu = true, bool hud = false, Action actionOnClick = null)
        {
            return new CustomButtonOption(id, title, menu, hud, actionOnClick);
        }
    }
}
