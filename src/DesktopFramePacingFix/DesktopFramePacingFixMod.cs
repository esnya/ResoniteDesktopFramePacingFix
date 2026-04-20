using System.Reflection;
using HarmonyLib;
using FrooxEngine;
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
    private static readonly Lock PatchStateLock = new();

    private static ModConfiguration? config;
    private static CancellationTokenSource? monitorCancellationTokenSource;
    private static Task? monitorTask;
    private static bool patchesApplied;

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

#if USE_RESONITE_HOT_RELOAD_LIB
    /// <summary>
    /// Removes Harmony patches before a hot reload cycle.
    /// </summary>
    public static void BeforeHotReload()
    {
        StopMonitoring();
        SetPatchesApplied(shouldPatch: false);
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
        StartMonitoring();

#if USE_RESONITE_HOT_RELOAD_LIB
        HotReloader.RegisterForHotReload(mod);
#endif
    }

    private static void HandleConfigurationChanged(ConfigurationChangedEvent _)
    {
        SubmitPacingPatch.ResetState();
        RefreshPatchState();
    }

    private static void StartMonitoring()
    {
        StopMonitoring();
        monitorCancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = monitorCancellationTokenSource.Token;
        monitorTask = Task.Run(() => MonitorPatchStateAsync(cancellationToken), cancellationToken);
    }

    private static void StopMonitoring()
    {
        CancellationTokenSource? cancellationTokenSource = monitorCancellationTokenSource;
        Task? runningMonitorTask = monitorTask;

        monitorCancellationTokenSource = null;
        monitorTask = null;

        if (cancellationTokenSource is null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
        try
        {
            runningMonitorTask?.Wait();
        }
        catch (AggregateException exception) when (exception.InnerExceptions.All(static inner => inner is TaskCanceledException))
        {
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }

    private static async Task MonitorPatchStateAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));
        RefreshPatchState();

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                RefreshPatchState();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void RefreshPatchState()
    {
        bool shouldPatch = false;

        if (Engine.Current?.InputInterface is InputInterface inputInterface)
        {
            SessionActivationState activation = SessionActivationState.Create(
                inputInterface.HeadOutputDevice,
                IsEnabled());

            shouldPatch = activation.ShouldUseWorkaround && !inputInterface.VR_Active;
        }

        SetPatchesApplied(shouldPatch);
    }

    private static void SetPatchesApplied(bool shouldPatch)
    {
        lock (PatchStateLock)
        {
            if (patchesApplied == shouldPatch)
            {
                return;
            }

            SubmitPacingPatch.ResetState();
            if (shouldPatch)
            {
                Harmony.PatchAll(Assembly);
            }
            else
            {
                Harmony.UnpatchAll(HarmonyId);
            }

            patchesApplied = shouldPatch;
        }

        DebugFunc(() => $"[DesktopFramePacingFix] Harmony patches {(shouldPatch ? "applied" : "removed")}.");
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
