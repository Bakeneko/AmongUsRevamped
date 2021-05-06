using AmongUsRevamped.Events;

namespace AmongUsRevamped.Mod
{
    public static class RevampedMod
    {
        public static void Load()
        {
            Options.Load();
            RegionsPatch.LoadRegions();

            Options.TestMode.ValueChanged += (_, e) =>
            {
                if ((bool)e.NewValue) TestMode.Load();
                else TestMode.Unload();
            };
            if (Options.TestMode.GetValue()) TestMode.Load();
        }

        public static void Unload()
        {
            TestMode.Unload();
        }
    }
}
