using HarmonyLib;
using UnhollowerBaseLib;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [HarmonyPatch]
    public class HatManagerPatch
    {
        /// <summary>
        /// Unlock all hats
        /// </summary>
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedHats))]
        [HarmonyPrefix]
        private static bool GetUnlockedHats(HatManager __instance, ref Il2CppReferenceArray<HatBehaviour> __result)
        {
            __result = (Il2CppReferenceArray<HatBehaviour>)__instance.AllHats.ToArray();
            return false;
        }

        /// <summary>
        /// Unlock all pets
        /// </summary>
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedPets))]
        [HarmonyPrefix]
        private static bool GetUnlockedPets(HatManager __instance, ref Il2CppReferenceArray<PetBehaviour> __result)
        {
            __result = (Il2CppReferenceArray<PetBehaviour>)__instance.AllPets.ToArray();
            return false;
        }

        /// <summary>
        /// Unlock all skins
        /// </summary>
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedSkins))]
        [HarmonyPrefix]
        private static bool GetUnlockedSkins(HatManager __instance, ref Il2CppReferenceArray<SkinData> __result)
        {
            __result = (Il2CppReferenceArray<SkinData>)__instance.AllSkins.ToArray();
            return false;
        }
    }
}
