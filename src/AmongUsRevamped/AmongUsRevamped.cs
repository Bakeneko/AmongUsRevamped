using System;
using System.IO;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Mod;
using AmongUsRevamped.UI;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Reactor;
using Reactor.Patches;

namespace AmongUsRevamped
{
    [BepInPlugin(Id, Name, Version)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class AmongUsRevamped : BasePlugin
    {
        public const string Id = "app.bakeneko.revamped";
        public const string Name = "Revamped";
        public const string Version = "0.1.0";
        public const byte Major = 0, Minor = 1, Patch = 0;

        public static AmongUsRevamped Instance { get { return PluginSingleton<AmongUsRevamped>.Instance; } }

        internal static ManualLogSource Logger { get { return ((BasePlugin)Instance).Log; } }

        public Harmony Harmony { get; } = new Harmony(Id);

        public string RevampedFolder => Path.Combine(Paths.PluginPath, AmongUsRevamped.Name);

        public override void Load()
        {
            PluginSingleton<AmongUsRevamped>.Instance = this;

            LogInfo($"Loading {Name} {Version}...");

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            try
            {
                if (!Directory.Exists(RevampedFolder)) Directory.CreateDirectory(RevampedFolder);

                LoadPatches();

                RegisterInIl2CppAttribute.Register();
                RegisterCustomRpcAttribute.Register(this);

                ReactorVersionShower.TextUpdated += (text) =>
                {
                    text.fontSize = 1.4f;
                    text.text = text.text.Insert(0, $"\n{Name} {Version}\n");
                };
                HudPosition.Load();
                PalettePatch.Load();
                RevampedMod.Load();
            }
            catch (Exception ex)
            {
                LogError($"An exception has occurred loading plugin:\r\n{ex}");
            }
        }

        public void LoadPatches()
        {
            try
            {
                Harmony.PatchAll();
            }
            catch (Exception ex)
            {
                LogError($"An exception has occurred loading patches:\r\n{ex}");
            }
        }

        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            return base.Unload();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError($"Unhandled exception:\r\n{ex}");
            }
        }

        public static new void Log(object message) => Logger.LogMessage(message?.ToString() ?? "");
        public static void LogInfo(object message) => Logger.LogInfo(message?.ToString() ?? "");
        public static void LogWarning(object message) => Logger.LogWarning(message?.ToString() ?? "");
        public static void LogError(object message) => Logger.LogError(message?.ToString() ?? "");

        public static void Debug(string msg, object obj, int line, string caller, string path)
        {
            Log($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {path.Split('\\').Last()} {caller}:{line}{(string.IsNullOrEmpty(msg) ? "" : " " + msg)} {obj}");
        }
    }
}
