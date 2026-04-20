using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
#if USE_RESONITE_HOT_RELOAD_LIB
using ResoniteHotReloadLib;
#endif

namespace DesktopFramePacingFix;

/// <summary>
/// ResoniteModLoader entry point for the desktop frame pacing workaround.
/// </summary>
public sealed class DesktopFramePacingFixMod : ResoniteMod
{
    private static readonly Assembly Assembly = typeof(DesktopFramePacingFixMod).Assembly;
    private static readonly string HarmonyId = $"com.nekometer.esnya.{Assembly.GetName().Name}";
    private static readonly Harmony Harmony = new(HarmonyId);

    private static ModConfiguration? config;

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnabledKey = new(
        "Enabled",
        "Enable the desktop frame pacing workaround for VR-capable sessions.",
        computeDefault: () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> ForegroundLimitEnabledKey = new(
        "ForegroundLimitEnabled",
        "Apply a fixed foreground frame limit while desktop mode is focused.",
        computeDefault: () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> MaximumForegroundFramerateKey = new(
        "MaximumForegroundFramerate",
        "Maximum foreground framerate used by the workaround.",
        computeDefault: () => 60,
        valueValidator: static value => value is >= 5 and <= 240);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> BackgroundLimitEnabledKey = new(
        "BackgroundLimitEnabled",
        "Apply a fixed background frame limit while desktop mode is unfocused.",
        computeDefault: () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> MaximumBackgroundFramerateKey = new(
        "MaximumBackgroundFramerate",
        "Maximum background framerate used by the workaround.",
        computeDefault: () => 30,
        valueValidator: static value => value is >= 5 and <= 144);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> VerboseLoggingKey = new(
        "VerboseLogging",
        "Log pacing mode transitions and effective limits.",
        computeDefault: () => false);

    /// <inheritdoc />
    public override string Name =>
        Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? Assembly.GetName().Name ?? string.Empty;

    /// <inheritdoc />
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

    /// <inheritdoc />
    public override string Version =>
        (Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty)
        .Split('+')[0];

    /// <inheritdoc />
    public override string Link =>
        Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(static metadata => metadata.Key == "RepositoryUrl")
            ?.Value ?? string.Empty;

    /// <inheritdoc />
    public override void OnEngineInit()
    {
        Initialize(this);
    }

    internal static bool IsEnabled()
    {
        return GetConfigValue(EnabledKey, fallback: true);
    }

    internal static bool GetForegroundLimitEnabled()
    {
        return GetConfigValue(ForegroundLimitEnabledKey, fallback: true);
    }

    internal static int GetMaximumForegroundFramerate()
    {
        return GetConfigValue(MaximumForegroundFramerateKey, fallback: 60);
    }

    internal static bool GetBackgroundLimitEnabled()
    {
        return GetConfigValue(BackgroundLimitEnabledKey, fallback: true);
    }

    internal static int GetMaximumBackgroundFramerate()
    {
        return GetConfigValue(MaximumBackgroundFramerateKey, fallback: 30);
    }

    internal static bool GetVerboseLogging()
    {
        return GetConfigValue(VerboseLoggingKey, fallback: false);
    }

#if USE_RESONITE_HOT_RELOAD_LIB
    /// <summary>
    /// Removes Harmony patches before a hot reload cycle.
    /// </summary>
    public static void BeforeHotReload()
    {
        SubmitPacingPatch.ResetState();
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Reinitializes the mod after a hot reload cycle.
    /// </summary>
    /// <param name="mod">The reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        Initialize(mod);
    }
#endif

    private static void Initialize(ResoniteMod mod)
    {
        ArgumentNullException.ThrowIfNull(mod);

        config = mod.GetConfiguration();
        if (config is not null)
        {
            config.OnThisConfigurationChanged -= HandleConfigurationChanged;
            config.OnThisConfigurationChanged += HandleConfigurationChanged;
        }

        SubmitPacingPatch.ResetState();

#if USE_RESONITE_HOT_RELOAD_LIB
        HotReloader.RegisterForHotReload(mod);
#endif

        Harmony.PatchAll(Assembly);
    }

    private static void HandleConfigurationChanged(ConfigurationChangedEvent _)
    {
        SubmitPacingPatch.ResetState();
    }

    private static T GetConfigValue<T>(ModConfigurationKey<T> key, T fallback)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (config is null)
        {
            return fallback;
        }

        if (!config.TryGetValue(key, out T? value))
        {
            return fallback;
        }

        return value!;
    }
}
