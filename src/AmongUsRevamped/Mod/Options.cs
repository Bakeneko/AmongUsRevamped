using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Options;
using Color = AmongUsRevamped.Colors.ColorPalette.Color;

namespace AmongUsRevamped.Mod
{
    public static class Options
    {
        public static CustomHeaderOption CustomGameSettings;
        public static CustomToggleOption TestMode;
        public static CustomToggleOption DisplayTasks;
        public static CustomToggleOption GhostsSeeTasks;

        public static void Load()
        {
            CustomSettings.Load();
            CustomOption.CreateExporter();
            CustomOption.CreateImporter();

            // Custom Game Settings
            CustomGameSettings = new CustomHeaderOption("customGameSettings", Color.Revamped.ToColorTag("Custom Game Settings"));
            TestMode = new CustomToggleOption("testMode", "Test mode", true, false, true);
            DisplayTasks = new CustomToggleOption("displayTasks", "Display remaining tasks", true, true, true);
            GhostsSeeTasks = new CustomToggleOption("ghostsSeeTasks", "Ghosts see remaining tasks", true, true, true);
            List<CustomOption> customGameSettingsBag = new(){ TestMode, DisplayTasks, GhostsSeeTasks };
            CustomGameSettings.OnValueChanged += (sender, e) => ExpandOptions(customGameSettingsBag, (bool)e.NewValue);
            ExpandOptions(customGameSettingsBag, false);
        }

        private static void ExpandOptions(List<CustomOption> options, bool expand = true)
        {
            foreach(CustomOption opt in options)
            {
                opt.MenuVisible = expand;
            }
        }

        public static class Values
        {
            public static bool TestMode => Options.TestMode.GetValue();
            public static bool DisplayTasks => Options.DisplayTasks.GetValue();
            public static bool GhostsSeeTasks => Options.GhostsSeeTasks.GetValue();
        }
    }
}
