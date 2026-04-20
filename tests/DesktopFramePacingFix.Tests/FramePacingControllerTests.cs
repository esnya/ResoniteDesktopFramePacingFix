namespace DesktopFramePacingFix.Tests;

public sealed class FramePacingControllerTests
{
    [Fact]
    public void ComputeRemainingDelayForSixtyFpsShouldUseSixteenMillisecondBudget()
    {
        TimeSpan delay = FramePacingController.ComputeRemainingDelay(60, TimeSpan.FromMilliseconds(5));

        Assert.InRange(delay.TotalMilliseconds, 11.5, 11.8);
    }

    [Fact]
    public void ComputeRemainingDelayForThirtyFpsShouldUseThirtyThreeMillisecondBudget()
    {
        TimeSpan delay = FramePacingController.ComputeRemainingDelay(30, TimeSpan.FromMilliseconds(10));

        Assert.InRange(delay.TotalMilliseconds, 23.2, 23.5);
    }

    [Fact]
    public void PrepareDelayWhenAlreadyLateShouldReturnZero()
    {
        FramePacingController controller = new(() => TimeSpan.Zero, static _ => { });

        Assert.Equal(TimeSpan.Zero, controller.PrepareDelay(60, TimeSpan.Zero));

        TimeSpan delay = controller.PrepareDelay(60, TimeSpan.FromMilliseconds(20));

        Assert.Equal(TimeSpan.Zero, delay);
    }

    [Fact]
    public void PrepareDelayWhenTargetChangesShouldResetSchedule()
    {
        FramePacingController controller = new(() => TimeSpan.Zero, static _ => { });

        Assert.Equal(TimeSpan.Zero, controller.PrepareDelay(60, TimeSpan.Zero));
        Assert.InRange(controller.PrepareDelay(60, TimeSpan.FromMilliseconds(5)).TotalMilliseconds, 11.5, 11.8);

        TimeSpan delay = controller.PrepareDelay(30, TimeSpan.FromMilliseconds(6));

        Assert.Equal(TimeSpan.Zero, delay);
        Assert.Equal(30, controller.CurrentTargetFramerate);
    }
}
