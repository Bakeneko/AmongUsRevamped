using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using Il2CppType = UnhollowerRuntimeLib.Il2CppType;

namespace AmongUsRevamped.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Fully read <paramref name="input"/> stream, can be used as workaround for il2cpp streams.
        /// </summary>
        public static byte[] ReadFully(this Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Sends <see cref="UnityWebRequest"/> and return task that finishes on <paramref name="request"/> completion.
        /// </summary>
        public static Task SendAsync(this UnityWebRequest request)
        {
            var task = new TaskCompletionSource<object>();

            request.Send().m_completeCallback = (Action<AsyncOperation>)(x =>
            {
                task.SetResult(null);
            });

            return task.Task;
        }

        private static readonly int _outline = Shader.PropertyToID("_Outline");
        private static readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int _addColor = Shader.PropertyToID("_AddColor");

        /// <summary>
        /// Sets color outline for renderers using default Among Us shader
        /// </summary>
        public static void SetOutline(this Renderer renderer, Color? color)
        {
            renderer.material.SetFloat(_outline, color.HasValue ? 1 : 0);

            if (color.HasValue)
            {
                renderer.material.SetColor(_outlineColor, color.Value);
                renderer.material.SetColor(_addColor, color.Value);
            }
        }

        public static IEnumerable<MethodBase> GetMethods(this Type type, BindingFlags bindingAttr, Type returnType, params Type[] parameterTypes)
        {
            return type.GetMethods(bindingAttr).Where(x => x.ReturnType == returnType && x.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes));
        }

        public static IEnumerable<MethodBase> GetMethods(this Type type, Type returnType, params Type[] parameterTypes)
        {
            return type.GetMethods(AccessTools.all, returnType, parameterTypes);
        }

        public static T LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }

        public static void Log(this object obj, string msg, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = "", [CallerFilePath] string path = "")
        {
            AmongUsRevamped.Debug(msg, obj, line, caller, path);
        }
    }
}
