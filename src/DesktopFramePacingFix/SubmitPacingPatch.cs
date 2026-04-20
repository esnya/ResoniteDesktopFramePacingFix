using System.Globalization;
using FrooxEngine;
using HarmonyLib;

namespace DesktopFramePacingFix;

[HarmonyPatch(typeof(RenderSystem), nameof(RenderSystem.SubmitFrame))]
internal static class SubmitPacingPatch
{
    private static readonly FramePacingController Controller = new();
    private static FramePacingDecision lastDecision;
    private static bool hasLastDecision;

    public static void Prefix(RenderSystem __instance)
    {
        ArgumentNullException.ThrowIfNull(__instance);

        if (__instance.State != RendererState.Rendering || __instance.Engine?.InputInterface is null)
        {
            ResetState();
            return;
        }

        InputInterface inputInterface = __instance.Engine.InputInterface;
        SessionActivationState activation = SessionActivationState.Create(
            inputInterface.HeadOutputDevice,
            DesktopFramePacingFixMod.IsEnabled());

        if (!activation.ShouldUseWorkaround)
        {
            ResetState();
            return;
        }

        FramePacingDecision decision = FramePacingPolicy.Build(
            activation,
            isVrActive: inputInterface.VR_Active,
            isFocused: FocusStateProvider.IsRendererFocused(__instance),
            foregroundLimitEnabled: DesktopFramePacingFixMod.GetForegroundLimitEnabled(),
            maximumForegroundFramerate: DesktopFramePacingFixMod.GetMaximumForegroundFramerate(),
            backgroundLimitEnabled: DesktopFramePacingFixMod.GetBackgroundLimitEnabled(),
            maximumBackgroundFramerate: DesktopFramePacingFixMod.GetMaximumBackgroundFramerate());

        LogDecisionIfNeeded(decision);
        Controller.Apply(decision.TargetFramerate);
    }

    public static void ResetState()
    {
        Controller.Reset();
        hasLastDecision = false;
        lastDecision = default;
    }

    private static void LogDecisionIfNeeded(FramePacingDecision decision)
    {
        if (hasLastDecision && lastDecision == decision)
        {
            return;
        }

        ResoniteModLoader.ResoniteMod.DebugFunc(
            () => $"[DesktopFramePacingFix] Mode={decision.Mode}, TargetFramerate={decision.TargetFramerate?.ToString(CultureInfo.InvariantCulture) ?? "None"}");

        lastDecision = decision;
        hasLastDecision = true;
    }
}
