using HarmonyLib;

namespace AmongUsRevamped.UI
{
    [HarmonyPatch]
    public partial class CooldownButton : GameButton
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager.Nested_5), nameof(HudManager.Nested_5.MoveNext))]
        private static void HudManagerShownIntroPatch() // Intro is fading
        {
            // Setup buttons initial cooldowns
            const float fadeTime = 0.2F;
            foreach (CooldownButton button in CooldownButtons) button.ApplyCooldown(button.InitialCooldownDuration - fadeTime);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        private static void MeetingHudStartPatch() // Meeting started
        {
            // End buttons effects if needed
            foreach (CooldownButton button in CooldownButtons) if (button.MeetingsEndEffect) button.EndEffect(true, false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        private static void ExileControllerWrapUpPatch() // Exile ended
        {
            // Game is ending, no need to reset cooldowns
            if (!DestroyableSingleton<TutorialManager>.InstanceExists && ShipStatus.Instance.IsGameOverDueToDeath()) return;

            // Reset cooldowns if needed
            foreach (CooldownButton button in CooldownButtons) if (button.CooldownAfterMeetings && !button.IsEffectActive) button.ApplyCooldown();
        }

    }
}
