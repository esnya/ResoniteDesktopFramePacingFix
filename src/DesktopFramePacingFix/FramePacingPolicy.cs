namespace DesktopFramePacingFix;

internal enum FramePacingMode
{
    None,
    Foreground,
    Background,
}

internal readonly record struct FramePacingDecision(FramePacingMode Mode, int? TargetFramerate)
{
    public static FramePacingDecision None { get; } = new(FramePacingMode.None, null);
}

internal static class FramePacingPolicy
{
    public static FramePacingDecision Build(
        SessionActivationState activation,
        bool isVrActive,
        bool isFocused,
        bool foregroundLimitEnabled,
        int maximumForegroundFramerate,
        bool backgroundLimitEnabled,
        int maximumBackgroundFramerate)
    {
        if (!activation.ShouldUseWorkaround || isVrActive)
        {
            return FramePacingDecision.None;
        }

        if (isFocused)
        {
            return foregroundLimitEnabled
                ? new FramePacingDecision(FramePacingMode.Foreground, maximumForegroundFramerate)
                : FramePacingDecision.None;
        }

        return backgroundLimitEnabled
            ? new FramePacingDecision(FramePacingMode.Background, maximumBackgroundFramerate)
            : FramePacingDecision.None;
    }
}
