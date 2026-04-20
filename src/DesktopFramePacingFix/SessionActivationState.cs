using Renderite.Shared;

namespace DesktopFramePacingFix;

internal readonly record struct SessionActivationState(bool IsVrCapableSession, bool IsModEnabled)
{
    public bool ShouldUseWorkaround => IsVrCapableSession && IsModEnabled;

    public static SessionActivationState Create(bool isVrViewSupported, bool isModEnabled)
    {
        return new SessionActivationState(isVrViewSupported, isModEnabled);
    }

    public static SessionActivationState Create(HeadOutputDevice headOutputDevice, bool isModEnabled)
    {
        return Create(headOutputDevice.IsVRViewSupported(), isModEnabled);
    }
}
