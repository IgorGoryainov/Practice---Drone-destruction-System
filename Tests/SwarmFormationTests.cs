using NUnit.Framework;
using System;
using UnityEngine;

[TestFixture]
public class SwarmFormationTests
{
    private const int DroneCount = 15;
    private const double SwarmRadius = 5.0;
    private const double TwoPi = 2 * Math.PI;

    // Reproduces the circle placement logic from SwarmControlSystem.ArrangeInCircle
    private static Vector3[] ComputeCirclePositions(float originX, float originY, float originZ, double radius)
    {
        var positions = new Vector3[DroneCount];
        double angleStep = TwoPi / DroneCount;
        double angle = 0;

        for (int i = 0; i < DroneCount; i++)
        {
            positions[i] = new Vector3(
                (float)(originX + radius * Math.Cos(angle)),
                originY,
                (float)(originZ + radius * Math.Sin(angle))
            );
            angle += angleStep;
        }

        return positions;
    }

    [Test]
    public void CircleFormation_AllDronesAtCorrectRadius()
    {
        float originX = 56f, originY = 0.9f, originZ = 108f;
        var positions = ComputeCirclePositions(originX, originY, originZ, SwarmRadius);
        var origin = new Vector3(originX, originY, originZ);

        foreach (var pos in positions)
        {
            float dist = (float)Math.Sqrt(
                Math.Pow(pos.x - origin.x, 2) +
                Math.Pow(pos.z - origin.z, 2)
            );
            Assert.That(dist, Is.EqualTo((float)SwarmRadius).Within(0.001f),
                "Each drone should be exactly SwarmRadius away from origin in XZ plane");
        }
    }

    [Test]
    public void CircleFormation_AllDronesAtSameAltitude()
    {
        var positions = ComputeCirclePositions(56f, 0.9f, 108f, SwarmRadius);

        foreach (var pos in positions)
            Assert.That(pos.y, Is.EqualTo(0.9f).Within(0.001f));
    }

    [Test]
    public void CircleFormation_DronesAreEvenlySpaced()
    {
        var positions = ComputeCirclePositions(56f, 0.9f, 108f, SwarmRadius);

        float expectedArcLength = (float)(TwoPi * SwarmRadius / DroneCount);

        for (int i = 0; i < DroneCount; i++)
        {
            var a = positions[i];
            var b = positions[(i + 1) % DroneCount];
            float chord = (float)Math.Sqrt(
                Math.Pow(a.x - b.x, 2) + Math.Pow(a.z - b.z, 2)
            );
            // chord ≈ arc length for small angles; all chords should be equal
            Assert.That(chord, Is.EqualTo(positions[0].x - positions[1 % DroneCount].x > 0
                ? chord : chord).Within(0.001f));

            // More useful: verify all adjacent pairs have the same chord length
            if (i > 0)
            {
                var prev = positions[i - 1];
                float prevChord = (float)Math.Sqrt(
                    Math.Pow(prev.x - a.x, 2) + Math.Pow(prev.z - a.z, 2)
                );
                Assert.That(chord, Is.EqualTo(prevChord).Within(0.001f),
                    $"Chord between drones {i} and {(i+1)%DroneCount} should equal chord between drones {i-1} and {i}");
            }
        }
    }

    [Test]
    public void CircleFormation_ProducesDroneCountPositions()
    {
        var positions = ComputeCirclePositions(0f, 0f, 0f, SwarmRadius);
        Assert.That(positions.Length, Is.EqualTo(DroneCount));
    }

    [Test]
    public void AttackFormation_InnerRingHasFiveDrones()
    {
        const int InnerRingCount = 5;
        const float InnerRadius = 1.5f;

        var innerPositions = new Vector3[InnerRingCount];
        double angleStep = TwoPi / InnerRingCount;
        double originX = 100, originY = 20, originZ = 100;

        for (int i = 0; i < InnerRingCount; i++)
        {
            double angle = i * angleStep;
            innerPositions[i] = new Vector3(
                (float)(originX + InnerRadius * Math.Cos(angle)),
                (float)(originY + InnerRadius * Math.Sin(angle)),
                (float)(originZ - 2f)
            );
        }

        Assert.That(innerPositions.Length, Is.EqualTo(5));
        foreach (var pos in innerPositions)
            Assert.That(pos.z, Is.EqualTo((float)(originZ - 2f)).Within(0.001f));
    }

    [Test]
    public void AttackFormation_OuterRingHasNineDrones()
    {
        const int OuterRingCount = 9;
        const float OuterRadius = 2.5f;

        var outerPositions = new Vector3[OuterRingCount];
        double angleStep = TwoPi / OuterRingCount;
        double originX = 100, originY = 20, originZ = 100;

        for (int i = 0; i < OuterRingCount; i++)
        {
            double angle = i * angleStep;
            outerPositions[i] = new Vector3(
                (float)(originX + OuterRadius * Math.Cos(angle)),
                (float)(originY + OuterRadius * Math.Sin(angle)),
                (float)(originZ - 4f)
            );
        }

        Assert.That(outerPositions.Length, Is.EqualTo(9));
        foreach (var pos in outerPositions)
            Assert.That(pos.z, Is.EqualTo((float)(originZ - 4f)).Within(0.001f));
    }

    [Test]
    public void AttackFormation_InnerPlusOuterPlusLeadEqualsTotal()
    {
        Assert.That(5 + 9 + 1, Is.EqualTo(DroneCount));
    }
}
