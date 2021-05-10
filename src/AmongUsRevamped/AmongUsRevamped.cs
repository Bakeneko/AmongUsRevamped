using System;
using System.IO;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Mod;
using AmongUsRevamped.UI;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmongUsRevamped
{
    [BepInPlugin(Id, Name, VersionString)]
    [BepInProcess("Among Us.exe")]
    public class AmongUsRevamped : BasePlugin
    {
        public const string Id = "app.bakeneko.revamped";
        public const string Name = "Revamped";
        public const string VersionString = "0.1.0";

        public static Version Version = new(VersionString);

        private static AmongUsRevamped _instance;
        public static AmongUsRevamped Instance
        {
            get => _instance ??= IL2CPPChainloader.Instance.Plugins.Values.Select(x => x.Instance).OfType<AmongUsRevamped>().Single();

            set
            {
                if (_instance != null)
                {
                    throw new Exception($"AmongUsRevamped instance is already set");
                }

                _instance = value;
            }
        }

        internal static ManualLogSource Logger { get { return ((BasePlugin)Instance).Log; } }

        public static System.Random Rand { get; } = new((int)DateTime.Now.Ticks);

        public static string RevampedFolder => Path.Combine(Paths.PluginPath, Name);

        public Harmony Harmony { get; } = new Harmony(Id);

        public CustomRpcManager CustomRpcManager { get; } = new CustomRpcManager();

        private BepInExLogListener LogListener { get; } = new BepInExLogListener();

        private GameObject GameObject;

        public override void Load()
        {
            Instance = this;

            LogInfo($"Loading {Name} {VersionString}...");

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            BepInEx.Logging.Logger.Listeners.Add(LogListener);

            try
            {
                if (!Directory.Exists(RevampedFolder)) Directory.CreateDirectory(RevampedFolder);

                RegisterInIl2CppAttribute.Register();
                RegisterCustomRpcAttribute.Register();

                GameObject = new GameObject(nameof(AmongUsRevamped)).DontDestroy();
                GameObject.AddComponent<Coroutines.Component>();

                LoadPatches();

                VersionShowerPatch.Load();

                // Skip splashscreen
                SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) => { if (scene.name == "SplashIntro") SceneManager.LoadScene("MainMenu"); }));

                HudPosition.Load();
                PalettePatch.Load();
                RevampedMod.Load();
            }
            catch (Exception ex)
            {
                LogError($"An exception has occurred loading plugin:\r\n{ex}");
            }
        }

        private void LoadPatches()
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
            RevampedMod.Unload();
            Harmony.UnpatchSelf();
            BepInEx.Logging.Logger.Listeners.Remove(LogListener);
            GameObject.Destroy();
            return base.Unload();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError($"Unhandled exception:\r\n{ex}");
            }
        }

        /// <inheritdoc cref="ManualLogSource.LogMessage"/>
        public static new void Log(object message) => Logger.LogMessage(message?.ToString() ?? "");

        /// <inheritdoc cref="ManualLogSource.LogDebug"/>
        public static void LogDebug(object message) => Logger.LogDebug(message?.ToString() ?? "");

        /// <inheritdoc cref="ManualLogSource.LogInfo"/>
        public static void LogInfo(object message) => Logger.LogInfo(message?.ToString() ?? "");

        /// <inheritdoc cref="ManualLogSource.LogWarning"/>
        public static void LogWarning(object message) => Logger.LogWarning(message?.ToString() ?? "");

        /// <inheritdoc cref="ManualLogSource.LogError"/>
        public static void LogError(object message) => Logger.LogError(message?.ToString() ?? "");

        /// <inheritdoc cref="ManualLogSource.LogFatal"/>
        public static void LogFatal(object message) => Logger.LogFatal(message?.ToString() ?? "");

        public static void Debug(string msg, object obj, int line, string caller, string path)
        {
            LogDebug($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {path.Split('\\').Last()} {caller}:{line}{(string.IsNullOrEmpty(msg) ? "" : " " + msg)} {obj}");
        }

        /// <summary>
        /// Listen 
        /// </summary>
        private class BepInExLogListener : ILogListener
        {
            public void LogEvent(object sender, LogEventArgs e)
            {
                if ((e.Level & (LogLevel.Fatal | LogLevel.Error)) == 0) return;
                if (e.Source.SourceName.Equals(Name)) return;
            }

            public void Dispose() { }
        }
    }
}
