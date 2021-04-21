using BepInEx;
using BepInEx.IL2CPP;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AmongUsRevamped.Utils
{
    public static class PluginUtils
    {
        /// <summary>
        /// Gets the "Id" string provided to the first derivative class of <see cref="BasePlugin"/> with the attribute <see cref="BepInPlugin"/> in the current call stack.
        /// </summary>
        /// <returns>A plugin id or <see cref="string.Empty"/></returns>
        public static string GetCallingPluginId(int frameIndex = 3)
        {
            StackTrace stackTrace = new StackTrace(frameIndex);
            for (int i = 0; i < stackTrace.GetFrames().Length; i++)
            {
                MethodBase method = stackTrace.GetFrame(i).GetMethod();
                Type type = method.ReflectedType;

                if (!type.IsClass || !type.IsSubclassOf(typeof(BasePlugin)) || type.IsAbstract) continue;

                foreach (CustomAttributeData attribute in type.CustomAttributes)
                {
                    if (attribute.AttributeType != typeof(BepInPlugin)) continue;

                    CustomAttributeTypedArgument arg = attribute.ConstructorArguments.FirstOrDefault();
                    if (arg == null || arg.ArgumentType != typeof(string) || arg.Value is not string value) continue;

                    return value;
                }
            }

            return string.Empty;
        }
    }
}
