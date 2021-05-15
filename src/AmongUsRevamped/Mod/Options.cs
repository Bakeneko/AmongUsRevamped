using System.Linq;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Options;
using Color = AmongUsRevamped.Colors.ColorPalette.Color;

namespace AmongUsRevamped.Mod
{
    public static class Options
    {
        // Custom options
        public static CustomHeaderOption CustomGameSettings;
        public static CustomToggleOption TestMode;
        public static CustomToggleOption DisplayTasks;
        public static CustomToggleOption GhostsSeeRoles;
        public static CustomToggleOption GhostsSeeTasks;
        public static CustomToggleOption AnonOnCommsSabotage;

        // Roles
        public static CustomHeaderOption RolesSettings;
        public static CustomNumberOption MaxCrewmateRoles;
        public static CustomNumberOption MaxNeutralRoles;
        public static CustomNumberOption MaxImpostorRoles;

        #region Crewmates

        // Engineer
        public static CustomHeaderOption EngineerSettings;
        public static CustomNumberOption EngineerSpawnRate;
        public static CustomStringOption EngineerRepairs;

        // Sheriff
        public static CustomHeaderOption SheriffSettings;
        public static CustomNumberOption SheriffSpawnRate;
        public static CustomNumberOption SheriffKillCooldown;
        public static CustomToggleOption SheriffCanKillSpy;

        // Snitch
        public static CustomHeaderOption SnitchSettings;
        public static CustomNumberOption SnitchSpawnRate;
        public static CustomNumberOption SnitchTasksLeftBeforeBusted;

        // Spy
        public static CustomHeaderOption SpySettings;
        public static CustomNumberOption SpySpawnRate;
        public static CustomNumberOption SpyGadgetCooldown;
        public static CustomNumberOption SpyGadgetDuration;

        // Time Lord
        public static CustomHeaderOption TimeLordSettings;
        public static CustomNumberOption TimeLordSpawnRate;
        public static CustomNumberOption TimeLordRewindCooldown;
        public static CustomNumberOption TimeLordRewindDuration;

        #endregion Crewmates

        #region Impostors

        // Camouflager
        public static CustomHeaderOption CamouflagerSettings;
        public static CustomNumberOption CamouflagerSpawnRate;
        public static CustomNumberOption CamouflagerCamouflageCooldown;
        public static CustomNumberOption CamouflagerCamouflageDuration;

        // Cleaner
        public static CustomHeaderOption CleanerSettings;
        public static CustomNumberOption CleanerSpawnRate;

        // Swooper
        public static CustomHeaderOption SwooperSettings;
        public static CustomNumberOption SwooperSpawnRate;
        public static CustomNumberOption SwooperSwoopCooldown;
        public static CustomNumberOption SwooperSwoopDuration;

        #endregion Impostors

        #region Neutrals

        // Jester
        public static CustomHeaderOption JesterSettings;
        public static CustomNumberOption JesterSpawnRate;

        #endregion Neutrals

        #region Modifiers

        // Modifiers
        public static CustomHeaderOption ModifiersSettings;
        public static CustomNumberOption MaxModifiers;
        public static CustomNumberOption DrunkSpawnRate;
        public static CustomNumberOption FlashSpawnRate;
        public static CustomNumberOption GiantSpawnRate;
        public static CustomNumberOption TorchSpawnRate;

        #endregion Modifiers

        public static void Load()
        {
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 15).ToArray();
            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat(3, 16).ToArray();
            CustomSettings.Load();
            CustomOption.CreateExporter();
            CustomOption.CreateImporter();

            // Custom options
            CustomGameSettings = new CustomHeaderOption("customGameSettings", Color.Revamped.ToColorTag("Custom Game Settings"));
            TestMode = new CustomToggleOption("testMode", "Test mode", true, false, true);
            DisplayTasks = new CustomToggleOption("displayTasks", "Display remaining tasks", true, true, true);
            GhostsSeeRoles = new CustomToggleOption("ghostsSeeRoles", "Ghosts see roles", true, true, true);
            GhostsSeeTasks = new CustomToggleOption("ghostsSeeTasks", "Ghosts see remaining tasks", true, true, true);
            AnonOnCommsSabotage = new CustomToggleOption("anonOnCommsSabotage", "Anonymize during comms sabotage", true, false, true);

            // Roles
            RolesSettings = new CustomHeaderOption("rolesSettings", "Roles Settings");
            MaxCrewmateRoles = new CustomNumberOption("maxCrewmateRoles", "Max crewmate roles", true, 0f, 0f, 9f, 1f, true);
            MaxNeutralRoles = new CustomNumberOption("maxNeutralRoles", "Max neutral roles", true, 0f, 0f, 3f, 1f, true);
            MaxImpostorRoles = new CustomNumberOption("maxImpostorRoles", "Max impostor roles", true, 0f, 0f, 3f, 1f, true);

            #region Crewmates

            // Engineer
            EngineerSettings = new CustomHeaderOption("engineerSettings", Color.RoleEngineer.ToColorTag("Engineer"));
            EngineerSpawnRate = new CustomNumberOption("engineerSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            EngineerRepairs = new CustomStringOption("engineerRepairs", "Repairs", true, new string[] { "1 per game", "2 per game", "3 per game", "1 per round" }, 0, true);

            // Sheriff
            SheriffSettings = new CustomHeaderOption("sheriffSettings", Color.RoleSheriff.ToColorTag("Sheriff"));
            SheriffSpawnRate = new CustomNumberOption("sheriffSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            SheriffKillCooldown = new CustomNumberOption("sheriffKillCooldown", "Kill cooldown", true, 30f, 10f, 60f, 2.5f, true, CustomNumberOption.SecondsStringFormat);
            SheriffCanKillSpy = new CustomToggleOption("sheriffCanKillSpy", "Can kill Spy", true, true, true);

            // Snitch
            SnitchSettings = new CustomHeaderOption("snitchSettings", Color.RoleSnitch.ToColorTag("Snitch"));
            SnitchSpawnRate = new CustomNumberOption("snitchSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            SnitchTasksLeftBeforeBusted = new CustomNumberOption("snitchTasksLeftBeforeBusted", "Tasks left before being busted", true, 1f, 0f, 5f, 1f, true);

            // Spy
            SpySettings = new CustomHeaderOption("spySettings", Color.RoleImpostor.ToColorTag("Spy"));
            SpySpawnRate = new CustomNumberOption("spySpawnRate", "Spawn rate (2+ Impostors)", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            SpyGadgetCooldown = new CustomNumberOption("spyGadgetCooldown", "Gadget cooldown", true, 30f, 0f, 60f, 5f, true, CustomNumberOption.SecondsStringFormat);
            SpyGadgetDuration = new CustomNumberOption("spyGadgetDuration", "Gadget duration", true, 10f, 2.5f, 30f, 2.5f, true, CustomNumberOption.SecondsStringFormat);

            // Time Lord
            TimeLordSettings = new CustomHeaderOption("timeLordSettings", Color.RoleTimeLord.ToColorTag("Time Lord"));
            TimeLordSpawnRate = new CustomNumberOption("timeLordSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            TimeLordRewindCooldown = new CustomNumberOption("timeLordRewindCooldown", "Rewind cooldown", true, 25f, 10f, 40f, 2.5f, true, CustomNumberOption.SecondsStringFormat);
            TimeLordRewindDuration = new CustomNumberOption("timeLordRewindDuration", "Rewind duration", true, 3f, 3f, 15f, 0.5f, true, CustomNumberOption.SecondsStringFormat);

            #endregion Crewmates

            #region Impostors

            // Camouflager
            CamouflagerSettings = new CustomHeaderOption("camouflagerSettings", Color.RoleImpostor.ToColorTag("Camouflager"));
            CamouflagerSpawnRate = new CustomNumberOption("camouflagerSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            CamouflagerCamouflageCooldown = new CustomNumberOption("camouflagerCamouflageCooldown", "Camouflage cooldown", true, 30f, 10f, 60f, 2.5f, true, CustomNumberOption.SecondsStringFormat);
            CamouflagerCamouflageDuration = new CustomNumberOption("camouflagerCamouflageDuration", "Camouflage duration", true, 10f, 1f, 20f, 0.5f, true, CustomNumberOption.SecondsStringFormat);

            // Cleaner
            CleanerSettings = new CustomHeaderOption("cleanerSettings", Color.RoleImpostor.ToColorTag("Cleaner"));
            CleanerSpawnRate = new CustomNumberOption("cleanerSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);

            // Swooper
            SwooperSettings = new CustomHeaderOption("swooperSettings", Color.RoleImpostor.ToColorTag("Swooper"));
            SwooperSpawnRate = new CustomNumberOption("swooperSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            SwooperSwoopCooldown = new CustomNumberOption("swooperSwoopCooldown", "Swoop cooldown", true, 25f, 10f, 40f, 2.5f, true, CustomNumberOption.SecondsStringFormat);
            SwooperSwoopDuration = new CustomNumberOption("swooperSwoopDuration", "Swoop duration", true, 10f, 5f, 15f, 1f, true, CustomNumberOption.SecondsStringFormat);
            
            #endregion Impostors

            #region Neutrals
            
            // Jester
            JesterSettings = new CustomHeaderOption("jesterSettings", Color.RoleJester.ToColorTag("Jester"));
            JesterSpawnRate = new CustomNumberOption("jesterSpawnRate", "Spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);

            #endregion Neutrals

            #region Modifiers
            
            // Modifier
            RolesSettings = new CustomHeaderOption("modifiersSettings", "Modifiers Settings");
            MaxModifiers = new CustomNumberOption("maxModifiers", "Max modifiers", true, 0f, 0f, 10f, 1f, true);
            DrunkSpawnRate = new CustomNumberOption("drunkSpawnRate", $"{Color.ModifierDrunk.ToColorTag("Drunk")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            FlashSpawnRate = new CustomNumberOption("flashSpawnRate", $"{Color.ModifierFlash.ToColorTag("Flash")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            GiantSpawnRate = new CustomNumberOption("flashSpawnRate", $"{Color.ModifierGiant.ToColorTag("Giant")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);
            TorchSpawnRate = new CustomNumberOption("torchSpawnRate", $"{Color.ModifierTorch.ToColorTag("Torch")} spawn rate", true, 0f, 0f, 100f, 10f, true, CustomNumberOption.PercentStringFormat);

            #endregion Modifiers
        }

        public static class Values
        {
            // Custom options
            public static bool TestMode => Options.TestMode.GetValue();
            public static bool DisplayTasks => Options.DisplayTasks.GetValue();
            public static bool GhostsSeeRoles => Options.GhostsSeeRoles.GetValue();
            public static bool GhostsSeeTasks => Options.GhostsSeeTasks.GetValue();
            public static bool AnonOnCommsSabotage => Options.AnonOnCommsSabotage.GetValue();

            // Roles
            public static int MaxCrewmateRoles => (int)Options.MaxCrewmateRoles.GetValue();
            public static int MaxNeutralRoles => (int)Options.MaxNeutralRoles.GetValue();
            public static int MaxImpostorRoles => (int)Options.MaxImpostorRoles.GetValue();

            #region Crewmates

            // Engineer
            public static float EngineerSpawnRate => Options.EngineerSpawnRate.GetValue();
            public static int EngineerRepairs => Options.EngineerRepairs.GetValue();

            // Sheriff
            public static float SheriffSpawnRate => Options.SheriffSpawnRate.GetValue();
            public static float SheriffKillCooldown => Options.SheriffKillCooldown.GetValue();
            public static bool SheriffCanKillSpy => Options.SheriffCanKillSpy.GetValue();

            // Snitch
            public static float SnitchSpawnRate => Options.SnitchSpawnRate.GetValue();
            public static float SnitchTasksLeftBeforeBusted => Options.SnitchTasksLeftBeforeBusted.GetValue();

            // Spy
            public static float SpySpawnRate => Options.SpySpawnRate.GetValue();
            public static float SpyGadgetCooldown => Options.SpyGadgetCooldown.GetValue();
            public static float SpyGadgetDuration => Options.SpyGadgetDuration.GetValue();

            // Time Lord
            public static float TimeLordSpawnRate => Options.TimeLordSpawnRate.GetValue();
            public static float TimeLordRewindCooldown => Options.TimeLordRewindCooldown.GetValue();
            public static float TimeLordRewindDuration => Options.TimeLordRewindDuration.GetValue();

            #endregion Crewmates

            #region Impostors

            // Camouflager
            public static float CamouflagerSpawnRate => Options.CamouflagerSpawnRate.GetValue();
            public static float CamouflagerCamouflageCooldown => Options.CamouflagerCamouflageCooldown.GetValue();
            public static float CamouflagerCamouflageDuration => Options.CamouflagerCamouflageDuration.GetValue();

            // Cleaner
            public static float CleanerSpawnRate => Options.CleanerSpawnRate.GetValue();

            // Swooper
            public static float SwooperSpawnRate => Options.SwooperSpawnRate.GetValue();
            public static float SwooperSwoopCooldown => Options.SwooperSwoopCooldown.GetValue();
            public static float SwooperSwoopDuration => Options.SwooperSwoopDuration.GetValue();

            #endregion Impostors

            #region Neutrals

            // Jester
            public static float JesterSpawnRate => Options.JesterSpawnRate.GetValue();

            #endregion Neutrals

            #region Modifiers

            // Modifiers
            public static int MaxModifiers => (int)Options.MaxModifiers.GetValue();
            public static float DrunkSpawnRate => Options.DrunkSpawnRate.GetValue();
            public static float FlashSpawnRate => Options.FlashSpawnRate.GetValue();
            public static float GiantSpawnRate => Options.GiantSpawnRate.GetValue();
            public static float TorchSpawnRate => Options.TorchSpawnRate.GetValue();

            #endregion Modifiers
        }
    }
}
