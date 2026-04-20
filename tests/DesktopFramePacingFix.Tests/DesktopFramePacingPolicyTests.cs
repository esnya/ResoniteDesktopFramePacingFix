namespace DesktopFramePacingFix.Tests;

public sealed class DesktopFramePacingPolicyTests
{
    [Fact]
    public void BuildWhenInactiveSessionShouldReturnNoLimit()
    {
        FramePacingDecision result = FramePacingPolicy.Build(
            activation: new SessionActivationState(IsVrCapableSession: false, IsModEnabled: true),
            isVrActive: false,
            isFocused: true,
            foregroundLimitEnabled: true,
            maximumForegroundFramerate: 60,
            backgroundLimitEnabled: true,
            maximumBackgroundFramerate: 30);

        Assert.Equal(FramePacingDecision.None, result);
    }

    [Fact]
    public void BuildWhenVrActiveShouldReturnNoLimit()
    {
        FramePacingDecision result = FramePacingPolicy.Build(
            activation: new SessionActivationState(IsVrCapableSession: true, IsModEnabled: true),
            isVrActive: true,
            isFocused: true,
            foregroundLimitEnabled: true,
            maximumForegroundFramerate: 60,
            backgroundLimitEnabled: true,
            maximumBackgroundFramerate: 30);

        Assert.Equal(FramePacingDecision.None, result);
    }

    [Fact]
    public void BuildWhenFocusedShouldReturnForegroundLimit()
    {
        FramePacingDecision result = FramePacingPolicy.Build(
            activation: new SessionActivationState(IsVrCapableSession: true, IsModEnabled: true),
            isVrActive: false,
            isFocused: true,
            foregroundLimitEnabled: true,
            maximumForegroundFramerate: 60,
            backgroundLimitEnabled: true,
            maximumBackgroundFramerate: 30);

        Assert.Equal(new FramePacingDecision(FramePacingMode.Foreground, 60), result);
    }

    [Fact]
    public void BuildWhenUnfocusedShouldReturnBackgroundLimit()
    {
        FramePacingDecision result = FramePacingPolicy.Build(
            activation: new SessionActivationState(IsVrCapableSession: true, IsModEnabled: true),
            isVrActive: false,
            isFocused: false,
            foregroundLimitEnabled: true,
            maximumForegroundFramerate: 60,
            backgroundLimitEnabled: true,
            maximumBackgroundFramerate: 30);

        Assert.Equal(new FramePacingDecision(FramePacingMode.Background, 30), result);
    }

    [Fact]
    public void BuildWhenForegroundLimitDisabledShouldReturnNoLimit()
    {
        FramePacingDecision result = FramePacingPolicy.Build(
            activation: new SessionActivationState(IsVrCapableSession: true, IsModEnabled: true),
            isVrActive: false,
            isFocused: true,
            foregroundLimitEnabled: false,
            maximumForegroundFramerate: 60,
            backgroundLimitEnabled: true,
            maximumBackgroundFramerate: 30);

        Assert.Equal(FramePacingDecision.None, result);
    }
}
