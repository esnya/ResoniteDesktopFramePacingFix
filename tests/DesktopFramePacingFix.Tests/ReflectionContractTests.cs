using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace DesktopFramePacingFix.Tests;

public sealed class ReflectionContractTests
{
    [Fact]
    public void RenderSystemShouldExposeExpectedSubmitFrameMethod()
    {
        MethodInfo? method = AccessTools.DeclaredMethod(typeof(RenderSystem), nameof(RenderSystem.SubmitFrame), []);

        Assert.NotNull(method);
    }

    [Fact]
    public void RenderSystemShouldExposeRendererWindowHandleProperty()
    {
        PropertyInfo? property = AccessTools.DeclaredProperty(typeof(RenderSystem), nameof(RenderSystem.RendererWindowHandle));

        Assert.NotNull(property);
    }

    [Fact]
    public void HarmonyPatchTypesShouldDeclarePatchMetadata()
    {
        Assert.NotEmpty(typeof(SubmitPacingPatch).GetCustomAttributes<HarmonyPatch>());
    }
}
