using System;
using System.Reflection;
using HarmonyLib;

namespace AmongUsRevamped
{
    /// <summary>
    /// Utility attribute for automatically registering CustomRpc
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterCustomRpcAttribute : Attribute
    {
        public uint Id { get; }

        public RegisterCustomRpcAttribute(uint id)
        {
            Id = id;
        }

        public static void Register()
        {
            Register(Assembly.GetCallingAssembly());
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<RegisterCustomRpcAttribute>();

                if (attribute != null)
                {
                    if (!type.IsSubclassOf(typeof(UnsafeCustomRpc)))
                    {
                        throw new InvalidOperationException($"Type {type.FullDescription()} has {nameof(RegisterCustomRpcAttribute)} but doesn't extend {nameof(UnsafeCustomRpc)}.");
                    }

                    var customRpc = (UnsafeCustomRpc)Activator.CreateInstance(type, attribute.Id);
                    AmongUsRevamped.Instance.CustomRpcManager.Register(customRpc);
                }
            }
        }
    }
}
