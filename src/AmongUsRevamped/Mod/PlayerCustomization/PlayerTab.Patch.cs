using HarmonyLib;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    class PlayerTabPatch
    {
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        public static class OnEnablePatch
        {
            public static void Postfix(PlayerTab __instance)
            {
                for (int i = 0; i < __instance.ColorChips.Count; i++)
                {
                    var chip = __instance.ColorChips.ToArray()[i];
                    chip.transform.localScale *= 0.7f;
                    chip.Button.OnClick.AddListener(Click(i));
                }
            }

            private static System.Action Click(int index)
            {
                void SetColor()
                {
                    // Save only vanilla colors
                    SaveManager.BodyColor = (byte)(index < 12 ? index : 0);
                }

                return SetColor;
            }
        }
    }
}
