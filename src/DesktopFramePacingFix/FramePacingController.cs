using System.Diagnostics;

namespace DesktopFramePacingFix;

internal sealed class FramePacingController
{
    private static readonly TimeSpan CoarseSleepThreshold = TimeSpan.FromMilliseconds(2);

    private readonly Func<TimeSpan> nowProvider;
    private readonly Action<TimeSpan> sleepAction;
    private TimeSpan? lastFrameTimestamp;

    public FramePacingController()
        : this(CreateStopwatchNowProvider(), SleepFor)
    {
    }

    internal FramePacingController(Func<TimeSpan> nowProvider, Action<TimeSpan> sleepAction)
    {
        this.nowProvider = nowProvider;
        this.sleepAction = sleepAction;
    }

    internal int? CurrentTargetFramerate { get; private set; }

    public void Apply(int? targetFramerate)
    {
        TimeSpan initialNow = nowProvider();
        TimeSpan delay = PrepareDelay(targetFramerate, initialNow);
        if (delay > TimeSpan.Zero)
        {
            sleepAction(delay);
        }

        Complete(nowProvider());
    }

    internal TimeSpan PrepareDelay(int? targetFramerate, TimeSpan now)
    {
        if (!targetFramerate.HasValue)
        {
            Reset();
            return TimeSpan.Zero;
        }

        if (targetFramerate.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetFramerate));
        }

        if (CurrentTargetFramerate != targetFramerate || lastFrameTimestamp is null)
        {
            CurrentTargetFramerate = targetFramerate;
            lastFrameTimestamp = now;
            return TimeSpan.Zero;
        }

        TimeSpan remainingDelay = ComputeRemainingDelay(
            targetFramerate.Value,
            now - lastFrameTimestamp.Value);

        if (remainingDelay <= TimeSpan.Zero)
        {
            lastFrameTimestamp = now;
        }

        return remainingDelay;
    }

    internal void Complete(TimeSpan now)
    {
        if (CurrentTargetFramerate is null)
        {
            return;
        }

        lastFrameTimestamp = now;
    }

    public void Reset()
    {
        CurrentTargetFramerate = null;
        lastFrameTimestamp = null;
    }

    internal static TimeSpan ComputeRemainingDelay(int targetFramerate, TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetFramerate);

        TimeSpan targetFrameDuration = TimeSpan.FromSeconds(1d / targetFramerate);
        TimeSpan remainingDelay = targetFrameDuration - elapsed;
        return remainingDelay > TimeSpan.Zero ? remainingDelay : TimeSpan.Zero;
    }

    private static Func<TimeSpan> CreateStopwatchNowProvider()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        return () => stopwatch.Elapsed;
    }

    private static void SleepFor(TimeSpan remainingDelay)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (remainingDelay - stopwatch.Elapsed > CoarseSleepThreshold)
        {
            Thread.Sleep(1);
        }

        while (stopwatch.Elapsed < remainingDelay)
        {
            Thread.SpinWait(64);
        }
    }
}
