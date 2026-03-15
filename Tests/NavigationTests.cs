using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class RLS_ActiveTests
{
    private Transform MakeTransform(float x, float y, float z) => new Transform(x, y, z);

    [Test]
    public void GetDistance_ReturnsZero_WhenTargetOutOfRange()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(0, 0, 2); // distance = 2, below MinDistance of 20
        var active = new Distance.RLS_Active(rls, target, -90, 90, 1f, 1f);

        Assert.That(active.getDistance(), Is.EqualTo(0f));
    }

    [Test]
    public void GetDistance_ReturnsZero_WhenTargetBeyondMaxRange()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(0, 0, 2000); // 2000 > MaxDistance 1500
        var active = new Distance.RLS_Active(rls, target, -90, 90, 1f, 1f);

        Assert.That(active.getDistance(), Is.EqualTo(0f));
    }

    [Test]
    public void GetAngle_ReturnsZero_WhenTargetOutsideSectorAngle()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(500, 0, 0); // angle ~90 degrees, outside sector -90..0
        var active = new Distance.RLS_Active(rls, target, -90, 0, 1f, 1f);

        Assert.That(active.getAngle(), Is.EqualTo(0f));
    }

    [Test]
    public void GetAngle_ReturnsNonZero_WhenTargetInsideSector()
    {
        var rls = MakeTransform(0, 0, 0);
        // Target at x=-500, z=100: angle = atan2(-500, 100) in degrees ≈ -78.7, inside -90..0
        var target = MakeTransform(-500, 0, 100);
        var active = new Distance.RLS_Active(rls, target, -90, 0, 1f, 1f);

        // With zero noise from stubs, getAngle returns the raw angle
        Assert.That(active.getAngle(), Is.Not.EqualTo(0f));
    }

    [Test]
    public void GetDistance_ReturnsZero_WhenElevationTooHigh()
    {
        var rls = MakeTransform(0, 0, 0);
        // target nearly directly above: high elevation angle > 13 degrees
        var target = MakeTransform(0, 500, 100);
        var active = new Distance.RLS_Active(rls, target, -90, 90, 1f, 1f);

        Assert.That(active.getDistance(), Is.EqualTo(0f));
    }
}

[TestFixture]
public class RLS_PassiveTests
{
    private Transform MakeTransform(float x, float y, float z) => new Transform(x, y, z);

    [Test]
    public void GetDistance_ReturnsZero_WhenTooClose()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(0, 0, 3); // distance = 3, below MinDistance of 5
        var passive = new Distance.RLS_Passive(rls, target, 1f, 1f);

        Assert.That(passive.getDistance(), Is.EqualTo(0f));
    }

    [Test]
    public void GetDistance_ReturnsZero_WhenBeyondMaxRange()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(0, 0, 8000); // 8000 > MaxDistance 7000
        var passive = new Distance.RLS_Passive(rls, target, 1f, 1f);

        Assert.That(passive.getDistance(), Is.EqualTo(0f));
    }

    [Test]
    public void GetDistance_ReturnsApproximateDistance_WhenInRange()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(0, 0, 100);
        var passive = new Distance.RLS_Passive(rls, target, 0f, 0f); // zero accuracy = no noise

        Assert.That(passive.getDistance(), Is.EqualTo(100f).Within(0.01f));
    }

    [Test]
    public void GetAngle_ReturnsZero_WhenTooClose()
    {
        var rls = MakeTransform(0, 0, 0);
        var target = MakeTransform(1, 0, 1); // distance ≈ 1.41, below MinDistance
        var passive = new Distance.RLS_Passive(rls, target, 1f, 1f);

        Assert.That(passive.getAngle(), Is.EqualTo(0f));
    }
}

[TestFixture]
public class NavigationSystemTests
{
    private Transform MakeTransform(float x, float y, float z) => new Transform(x, y, z);

    private Distance.NavigationSystem BuildNavSystem(
        Transform drone,
        int[] sectors = null,
        float[] rlsPositions = null
    )
    {
        // Place 4 active RLS sensors at corners of a 200m square around origin
        var rls1 = new Distance.RLS_Active(MakeTransform(-100, 0, -100), drone, -90, 0, 0f, 0f);
        var rls2 = new Distance.RLS_Active(MakeTransform(-100, 0, 100), drone, -180, -90, 0f, 0f);
        var rls3 = new Distance.RLS_Active(MakeTransform(100, 0, 100), drone, 90, 180, 0f, 0f);
        var rls4 = new Distance.RLS_Active(MakeTransform(100, 0, -100), drone, 0, 90, 0f, 0f);
        var rls5 = new Distance.RLS_Passive(MakeTransform(0, 0, 0), drone, 0f, 0f);
        return new Distance.NavigationSystem(rls1, rls2, rls3, rls4, rls5);
    }

    [Test]
    public void PossibilityGetTargetCoords_ReturnsFalse_WhenNoRLSCanSeeTarget()
    {
        // Target at distance 3 (too close for passive), and out of active range
        var drone = MakeTransform(0, 0, 3);
        var nav = BuildNavSystem(drone);

        Assert.That(nav.PossibilityGetTargetCoords(), Is.False);
    }

    [Test]
    public void GetTargetXYZ_ReturnZero_WhenTargetNotVisible()
    {
        var drone = MakeTransform(0, 0, 3);
        var nav = BuildNavSystem(drone);

        Assert.That(nav.getTargetX(), Is.EqualTo(0d));
        Assert.That(nav.getTargetY(), Is.EqualTo(0d));
        Assert.That(nav.getTargetZ(), Is.EqualTo(0d));
    }
}
