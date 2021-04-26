using AmongUsRevamped.Options;

namespace AmongUsRevamped.Mod
{
    public static class Options
    {
        public static void Load()
        {
            CustomSettings.Load();
            CustomOption.CreateExporter();
            CustomOption.CreateImporter();
        }
    }
}
