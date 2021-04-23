using System.Runtime.CompilerServices;

namespace AmongUsRevamped.Extensions
{
    public static class ObjectExtensions
    {
        public static void Log(this object obj, string msg = "", [CallerLineNumber] int line = 0,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string path = "")
        {
            AmongUsRevamped.Debug(msg, obj, line, caller, path);
        }
    }
}
