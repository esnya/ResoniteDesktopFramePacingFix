using Renderite.Shared;

namespace DesktopFramePacingFix.Tests;

public sealed class SessionActivationStateTests
{
    [Fact]
    public void CreateWhenVrCapableShouldEnableWorkaroundIfModEnabled()
    {
        SessionActivationState state = SessionActivationState.Create(HeadOutputDevice.SteamVR, isModEnabled: true);

        Assert.True(state.IsVrCapableSession);
        Assert.True(state.ShouldUseWorkaround);
    }

    [Fact]
    public void CreateWhenScreenOnlyShouldStayInactive()
    {
        SessionActivationState state = SessionActivationState.Create(HeadOutputDevice.Screen, isModEnabled: true);

        Assert.False(state.IsVrCapableSession);
        Assert.False(state.ShouldUseWorkaround);
    }
}
