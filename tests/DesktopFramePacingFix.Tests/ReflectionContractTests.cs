using Mono.Cecil;

namespace DesktopFramePacingFix.Tests;

public sealed class ReflectionContractTests
{
    [Fact]
    public void RenderSystemShouldExposeExpectedSubmitFrameMethod()
    {
        using AssemblyDefinition frooxEngineAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("FrooxEngine.dll"));
        TypeDefinition renderSystem = GetRequiredType(frooxEngineAssembly, "FrooxEngine.RenderSystem");
        MethodDefinition? method = renderSystem.Methods.FirstOrDefault(static method =>
            method.Name == "SubmitFrame" && !method.HasParameters);

        Assert.NotNull(method);
    }

    [Fact]
    public void RenderSystemShouldExposeRendererWindowHandleProperty()
    {
        using AssemblyDefinition frooxEngineAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("FrooxEngine.dll"));
        TypeDefinition renderSystem = GetRequiredType(frooxEngineAssembly, "FrooxEngine.RenderSystem");
        PropertyDefinition? property = renderSystem.Properties.FirstOrDefault(static property =>
            property.Name == "RendererWindowHandle");

        Assert.NotNull(property);
    }

    [Fact]
    public void HarmonyPatchTypesShouldDeclarePatchMetadata()
    {
        using AssemblyDefinition modAssembly = AssemblyDefinition.ReadAssembly(GetAssemblyPath("DesktopFramePacingFix.dll"));
        TypeDefinition patchType = GetRequiredType(modAssembly, "DesktopFramePacingFix.SubmitPacingPatch");

        Assert.Contains(
            patchType.CustomAttributes,
            static attribute => attribute.AttributeType.FullName is "HarmonyLib.HarmonyPatch");
    }

    private static string GetAssemblyPath(string assemblyFileName)
    {
        return Path.Combine(AppContext.BaseDirectory, assemblyFileName);
    }

    private static TypeDefinition GetRequiredType(AssemblyDefinition assembly, string fullName)
    {
        return assembly.MainModule.GetType(fullName)
            ?? throw new InvalidOperationException($"Type '{fullName}' was not found in '{assembly.MainModule.FileName}'.");
    }
}
