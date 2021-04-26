using AmongUsRevamped.Events;

namespace AmongUsRevamped.Mod
{
    public static class RevampedMod
    {
        public static void Load()
        {
            Options.Load();
            RegionMenuPatch.LoadRegions();
        }
    }
}
