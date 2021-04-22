using AmongUsRevamped.Options;

namespace AmongUsRevamped.Mod
{
    public static class Options
    {
        public static void Load()
        {
            CustomOption.CreateExporter();
            CustomOption.CreateImporter();
        }
    }
}
