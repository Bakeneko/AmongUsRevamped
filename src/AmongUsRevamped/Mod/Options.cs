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
        public static CustomToggleOption GhostsSeeRoles;
        public static CustomToggleOption GhostsSeeTasks;

        public static CustomHeaderOption RolesSettings;
        public static CustomNumberOption MaxCrewmateRoles;
        public static CustomNumberOption MaxImpostorRoles;

        public static CustomHeaderOption SheriffSettings;
        public static CustomNumberOption SheriffSpawnRate;
        public static CustomNumberOption SheriffKillCooldown;

        public static CustomHeaderOption JesterSettings;
        public static CustomNumberOption JesterSpawnRate;

        public static CustomHeaderOption ModifiersSettings;
        public static CustomNumberOption MaxModifiers;
        public static CustomNumberOption DrunkSpawnRate;
        public static CustomNumberOption FlashSpawnRate;
        public static CustomNumberOption TorchSpawnRate;

        public static void Load()
        {
            CustomSettings.Load();
            CustomOption.CreateExporter();
            CustomOption.CreateImporter();

            // Custom Game Settings
            CustomGameSettings = new CustomHeaderOption("customGameSettings", Color.Revamped.ToColorTag("Custom Game Settings"));
            TestMode = new CustomToggleOption("testMode", "Test mode", true, false, true);
            DisplayTasks = new CustomToggleOption("displayTasks", "Display remaining tasks", true, true, true);
            GhostsSeeRoles = new CustomToggleOption("ghostsSeeRoles", "Ghosts see roles", true, true, true);
            GhostsSeeTasks = new CustomToggleOption("ghostsSeeTasks", "Ghosts see remaining tasks", true, true, true);

            RolesSettings = new CustomHeaderOption("rolesSettings", "Roles Settings");
            MaxCrewmateRoles = new CustomNumberOption("maxCrewmateRoles", "Max crewmate roles", true, 0f, 0f, 9f, 1f, true);
            MaxImpostorRoles = new CustomNumberOption("maxImpostorRoles", "Max impostor roles", true, 0f, 0f, 3f, 1f, true);

            SheriffSettings = new CustomHeaderOption("sheriffSettings", Color.RoleSheriff.ToColorTag("Sheriff"));
            SheriffSpawnRate = new CustomNumberOption("sheriffSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            SheriffKillCooldown = new CustomNumberOption("sheriffKillCooldown", "Kill cooldown", true, 30f, 10f, 60f, 2.5f, true, CustomNumberOption.SecondsStringFormat);

            JesterSettings = new CustomHeaderOption("jesterSettings", Color.RoleJester.ToColorTag("Jester"));
            JesterSpawnRate = new CustomNumberOption("jesterSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);

            RolesSettings = new CustomHeaderOption("modifiersSettings", "Modifiers Settings");
            MaxModifiers = new CustomNumberOption("maxModifiers", "Max modifiers", true, 0f, 0f, 10f, 1f, true);
            DrunkSpawnRate = new CustomNumberOption("drunkSpawnRate", $"{Color.ModifierDrunk.ToColorTag("Drunk")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            FlashSpawnRate = new CustomNumberOption("flashSpawnRate", $"{Color.ModifierFlash.ToColorTag("Flash")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            TorchSpawnRate = new CustomNumberOption("torchSpawnRate", $"{Color.ModifierTorch.ToColorTag("Torch")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
        }

        public static class Values
        {
            public static bool TestMode => Options.TestMode.GetValue();
            public static bool DisplayTasks => Options.DisplayTasks.GetValue();
            public static bool GhostsSeeRoles => Options.GhostsSeeRoles.GetValue();
            public static bool GhostsSeeTasks => Options.GhostsSeeTasks.GetValue();

            public static int MaxCrewmateRoles => (int)Options.MaxCrewmateRoles.GetValue();
            public static int MaxImpostorRoles => (int)Options.MaxImpostorRoles.GetValue();

            public static float SheriffSpawnRate => Options.SheriffSpawnRate.GetValue();
            public static float SheriffKillCooldown => Options.SheriffKillCooldown.GetValue();

            public static float JesterSpawnRate => Options.JesterSpawnRate.GetValue();

            public static int MaxModifiers => (int)Options.MaxModifiers.GetValue();
            public static float DrunkSpawnRate => Options.DrunkSpawnRate.GetValue();
            public static float FlashSpawnRate => Options.FlashSpawnRate.GetValue();
            public static float TorchSpawnRate => Options.TorchSpawnRate.GetValue();
        }
    }
}
