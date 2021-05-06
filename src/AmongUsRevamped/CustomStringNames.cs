using System.Collections.Generic;
using System.Collections.ObjectModel;
using HarmonyLib;
using UnhollowerBaseLib;

namespace AmongUsRevamped
{
    [HarmonyPatch]
    public class CustomStringNames
    {
        private static int _lastId = -1;

        private static readonly Dictionary<int, CustomStringNames> _map = new();

        public static IReadOnlyDictionary<int, CustomStringNames> Map => new ReadOnlyDictionary<int, CustomStringNames>(_map);

        public static CustomStringNames Register(string value)
        {
            var customStringName = new CustomStringNames(_lastId--, value);
            _map.Add(customStringName.Id, customStringName);

            return customStringName;
        }

        public int Id { get; }

        public string Value { get; }

        private CustomStringNames(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public static implicit operator StringNames(CustomStringNames name) => (StringNames)name.Id;
        public static explicit operator CustomStringNames(StringNames name) => _map.TryGetValue((int)name, out CustomStringNames stringName) ? stringName : null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
        private static bool TranslationControllerGetStringPatch([HarmonyArgument(0)] StringNames stringId, [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts, ref string __result)
        {
            var customStringName = (CustomStringNames)stringId;
            if (customStringName != null)
            {
                __result = string.Format(customStringName.Value, parts);
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetStringWithDefault))]
        private static bool TranslationControllerGetStringWithDefaultPatch([HarmonyArgument(0)] StringNames stringId, [HarmonyArgument(2)] Il2CppReferenceArray<Il2CppSystem.Object> parts, ref string __result)
        {
            var customStringName = (CustomStringNames)stringId;
            if (customStringName != null)
            {
                __result = string.Format(customStringName.Value, parts);
                return false;
            }

            return true;
        }
    }
}
