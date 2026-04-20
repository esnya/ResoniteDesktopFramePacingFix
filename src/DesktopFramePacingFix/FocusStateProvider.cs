using System.Diagnostics;
using System.Runtime.InteropServices;
using FrooxEngine;

namespace DesktopFramePacingFix;

internal static class FocusStateProvider
{
    public static bool IsRendererFocused(RenderSystem renderSystem)
    {
        ArgumentNullException.ThrowIfNull(renderSystem);

        if (!OperatingSystem.IsWindows())
        {
            return true;
        }

        nint foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return true;
        }

        nint rendererWindow = renderSystem.RendererWindowHandle;
        if (rendererWindow == nint.Zero)
        {
            rendererWindow = Process.GetCurrentProcess().MainWindowHandle;
        }

        return rendererWindow == nint.Zero || rendererWindow == foregroundWindow;
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();
}
