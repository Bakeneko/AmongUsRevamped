using AmongUsRevamped.Events;

namespace AmongUsRevamped.Mod
{
    public static class RevampedMod
    {
        public static void Load()
        {
            Options.Load();
            RegionMenuPatch.LoadRegions();

            Options.TestMode.ValueChanged += (_, e) =>
            {
                if ((bool)e.NewValue) TestMode.Load();
                else TestMode.Unload();
            };
            if (Options.TestMode.GetValue()) TestMode.Load();
        }
    }
}
