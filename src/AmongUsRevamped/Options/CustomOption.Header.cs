using System;

namespace AmongUsRevamped.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomButtonOption"/>, handling option headers.
    /// </summary>
    public class CustomHeaderOption : CustomButtonOption
    {
        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        /// <param name="menu">Header will be visible in the lobby options menu</param>
        /// <param name="hud">Header will appear in the HUD (option list) in the lobby</param>
        /// <param name="actionOnClick">Action to execute on click</param>
        public CustomHeaderOption(string id, string title, bool menu = true, bool hud = true, Action actionOnClick = null) : base(id, title, menu, hud, actionOnClick)
        {
        }

        protected override bool GameObjectCreated(OptionBehaviour obj)
        {
            if (obj is ToggleOption toggle)
            {
                toggle.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
                toggle.transform.FindChild("Background")?.gameObject?.SetActive(false);
            }
            else if (obj is StringOption str)
            {
                str.transform.FindChild("Background")?.gameObject?.SetActive(false);
            }
            else return false;

            return UpdateGameObject();
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        /// <param name="menu">Header will be visible in the lobby options menu</param>
        /// <param name="hud">Header will appear in the HUD (option list) in the lobby</param>
        /// <param name="actionOnClick">Action to execute on click</param>
        public static CustomHeaderOption AddHeader(string id, string title, bool menu = true, bool hud = true, Action actionOnClick = null)
        {
            return new CustomHeaderOption(id, title, menu, hud, actionOnClick);
        }
    }
}
