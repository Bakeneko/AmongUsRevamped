using AmongUsRevamped.Mod.PlayerCustomization;
using UnhollowerRuntimeLib;

namespace AmongUsRevamped.Mod
{
    public static class RevampedMod
    {
        public static void Load()
        {
            Options.Load();
            ClassInjector.RegisterTypeInIl2Cpp<PlayerColorBehaviour>();
        }
    }
}
