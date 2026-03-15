using System;
using UnityEngine;

public class SwarmControlSystem : MonoBehaviour
{
    public Transform targetMarker;
    public Transform[] drones = new Transform[15];

    public float swarmSpeed;

    private Distance navigationSystem;
    private Vector3 previousSwarmPosition;

    private const int DroneCount = 15;
    private const float GroundOriginX = 56.0f;
    private const float GroundOriginY = 0.9f;
    private const float GroundOriginZ = 108.0f;
    private const float AltitudeOriginY = 10.0f;
    private const float SwarmRadius = 5.0f;
    private const float TakeoffAltitudeThreshold = 9f;
    private const float ApproachSpeed = 0.5f;
    private const float TakeoffLerpStep = 0.01f;

    private const int InnerRingCount = 5;
    private const int OuterRingCount = 9;
    private const float InnerRingRadius = 1.5f;
    private const float OuterRingRadius = 2.5f;
    private const float InnerRingOffset = -2f;
    private const float OuterRingOffset = -4f;

    void Start()
    {
        navigationSystem = FindObjectOfType<Distance>();
        ArrangeInCircle(GroundOriginX, GroundOriginY, GroundOriginZ, SwarmRadius, TakeoffLerpStep, instant: true);
        previousSwarmPosition = drones[0].position;
    }

    void FixedUpdate()
    {
        double targetX = navigationSystem.Target_X;
        double targetY = navigationSystem.Target_Y;
        double targetZ = navigationSystem.Target_Z;

        swarmSpeed = (drones[0].position - previousSwarmPosition).magnitude / Time.deltaTime;
        previousSwarmPosition = drones[0].position;

        bool targetDetected = targetX != 0;
        bool airborne = previousSwarmPosition.y >= TakeoffAltitudeThreshold;

        if (targetDetected && !airborne)
        {
            ArrangeInCircle(GroundOriginX, AltitudeOriginY, GroundOriginZ, SwarmRadius, TakeoffLerpStep, instant: false);
        }

        if (targetDetected && airborne)
        {
            float smooth = 1 - Mathf.Pow(0.5f, Time.deltaTime * ApproachSpeed);
            MoveToAttackFormation(targetX, targetY, targetZ, smooth);
        }
    }

    private void ArrangeInCircle(float originX, float originY, float originZ, double radius, float lerpStep, bool instant)
    {
        double angleStep = 2 * Math.PI / DroneCount;
        double angle = 0;

        for (int i = 0; i < DroneCount; i++)
        {
            Vector3 target = new Vector3(
                (float)(originX + radius * Math.Cos(angle)),
                originY,
                (float)(originZ + radius * Math.Sin(angle))
            );

            drones[i].position = instant ? target : Vector3.Lerp(drones[i].position, target, lerpStep);
            angle += angleStep;
        }
    }

    private void MoveToAttackFormation(double originX, double originY, double originZ, float smooth)
    {
        double innerAngleStep = 2 * Math.PI / InnerRingCount;
        double outerAngleStep = 2 * Math.PI / OuterRingCount;

        for (int i = 0; i < InnerRingCount; i++)
        {
            double angle = i * innerAngleStep;
            Vector3 target = new Vector3(
                (float)(originX + InnerRingRadius * Math.Cos(angle)),
                (float)(originY + InnerRingRadius * Math.Sin(angle)),
                (float)(originZ + InnerRingOffset)
            );
            drones[i].position = Vector3.Lerp(drones[i].position, target, smooth);
        }

        for (int i = 0; i < OuterRingCount; i++)
        {
            double angle = i * outerAngleStep;
            Vector3 target = new Vector3(
                (float)(originX + OuterRingRadius * Math.Cos(angle)),
                (float)(originY + OuterRingRadius * Math.Sin(angle)),
                (float)(originZ + OuterRingOffset)
            );
            drones[InnerRingCount + i].position = Vector3.Lerp(drones[InnerRingCount + i].position, target, smooth);
        }

        Vector3 leadTarget = new Vector3((float)originX, (float)originY, (float)originZ);
        drones[DroneCount - 1].position = Vector3.Lerp(drones[DroneCount - 1].position, leadTarget, smooth);
    }
}
