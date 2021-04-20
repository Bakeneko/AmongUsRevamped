using System;
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

        public static AmongUsRevamped Instance { get { return PluginSingleton<AmongUsRevamped>.Instance; } }

        internal static ManualLogSource Logger { get { return Instance.Log; } }

        public Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            PluginSingleton<AmongUsRevamped>.Instance = this;

            Logger.LogInfo($"Loading {Name} {Version}...");

            try
            {
                Harmony.PatchAll();

                RegisterInIl2CppAttribute.Register();
                RegisterCustomRpcAttribute.Register(this);

                ReactorVersionShower.TextUpdated += (text) =>
                {
                    string txt = text.text;
                    int index = txt.IndexOf('\n');
                    txt = txt.Insert(index == -1 ? txt.Length - 1 : index, $"\n{Name} {Version}");
                    text.text = txt;
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"An exception has occurred loading plugin: {ex}");
            }
        }
    }
}
