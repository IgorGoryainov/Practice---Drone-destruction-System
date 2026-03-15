using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Distance : MonoBehaviour
{
    public Transform RLS1;
    public Transform RLS2;
    public Transform RLS3;
    public Transform RLS4;
    public Transform RLS5;
    public Transform Drone;

    public float accuracyDistanceActive = 1f;
    public float accuracyAngleActive = 3f;
    public float accuracyDistancePassive = 3f;
    public float accuracyAnglePassive = 3f;

    public double Target_X;
    public double Target_Y;
    public double Target_Z;
    public double Nav_Error;

    public class RLS_Active
    {
        public Transform RLSPos;

        private Transform target;
        private int angleLimit_1;
        private int angleLimit_2;
        private float angle;
        private float angleRadians;
        private double elevationAngle;
        private float distance;
        private float accuracyDistance;
        private float accuracyAngle;

        private const float MinDistance = 20f;
        private const float MaxDistance = 1500f;
        private const float MaxElevationAngle = 13f;

        public RLS_Active(
            Transform rls,
            Transform targetDrone,
            int angle1,
            int angle2,
            float accuracyDist,
            float accuracyAng
        )
        {
            accuracyDistance = accuracyDist;
            accuracyAngle = accuracyAng;
            RLSPos = rls;
            target = targetDrone;
            angleLimit_1 = angle1;
            angleLimit_2 = angle2;
            float xSide = target.position.x - RLSPos.position.x;
            float zSide = target.position.z - RLSPos.position.z;
            float ySide = target.position.y - RLSPos.position.y;
            elevationAngle = Mathf.Atan2(ySide, Mathf.Sqrt(Mathf.Pow(xSide, 2) + Mathf.Pow(zSide, 2))) * Mathf.Rad2Deg;
            angle = Mathf.Atan2(xSide, zSide) * Mathf.Rad2Deg;
            angleRadians = Mathf.Atan2(xSide, zSide);
            distance = Vector3.Distance(RLSPos.position, target.position);
        }

        private bool IsVisible()
        {
            return angle >= angleLimit_1
                && angle <= angleLimit_2
                && distance >= MinDistance
                && distance <= MaxDistance;
        }

        public float getAngle()
        {
            return IsVisible()
                ? angle + Random.Range(-accuracyAngle, accuracyAngle)
                : 0;
        }

        public float getAngleRadians()
        {
            return IsVisible()
                ? angleRadians + Random.Range(-accuracyAngle, accuracyAngle) * Mathf.Deg2Rad
                : 0;
        }

        public float getDistance()
        {
            if (!IsVisible() || Math.Abs(elevationAngle) >= MaxElevationAngle)
                return 0;
            return distance + Random.Range(-accuracyDistance, accuracyDistance);
        }
    }

    public class RLS_Passive
    {
        public Transform RLSPos;

        private Transform target;
        private float angle;
        private float angleRadians;
        private float distance;
        private float accuracyDistance;
        private float accuracyAngle;

        private const float MinDistance = 5f;
        private const float MaxDistance = 7000f;

        public RLS_Passive(
            Transform rls,
            Transform targetDrone,
            float accuracyDist,
            float accuracyAng
        )
        {
            accuracyDistance = accuracyDist;
            accuracyAngle = accuracyAng;
            RLSPos = rls;
            target = targetDrone;
            float xSide = target.position.x - RLSPos.position.x;
            float zSide = target.position.z - RLSPos.position.z;
            angle = Mathf.Atan2(xSide, zSide) * Mathf.Rad2Deg;
            angleRadians = Mathf.Atan2(xSide, zSide);
            distance = Vector3.Distance(RLSPos.position, target.position);
        }

        private bool IsVisible()
        {
            return distance > MinDistance && distance <= MaxDistance;
        }

        public float getAngle()
        {
            return IsVisible()
                ? angle + Random.Range(-accuracyAngle, accuracyAngle)
                : 0;
        }

        public float getAngleRadians()
        {
            return IsVisible()
                ? angleRadians + Random.Range(-accuracyAngle, accuracyAngle) * Mathf.Deg2Rad
                : 0;
        }

        public float getDistance()
        {
            return IsVisible()
                ? distance + Random.Range(-accuracyDistance, accuracyDistance)
                : 0;
        }
    }

    public class NavigationSystem
    {
        private struct TargetCoords
        {
            public double x;
            public double y;
            public double z;
            public double yAbove;
        }

        private RLS_Active RLS1_A;
        private RLS_Active RLS2_A;
        private RLS_Active RLS3_A;
        private RLS_Active RLS4_A;
        private RLS_Passive RLS5_P;

        public double tan_1;
        public double tan_2;

        private TargetCoords targetCoords;

        private const int SampleCount = 20;

        public NavigationSystem(
            RLS_Active rls1,
            RLS_Active rls2,
            RLS_Active rls3,
            RLS_Active rls4,
            RLS_Passive rls5
        )
        {
            RLS1_A = rls1;
            RLS2_A = rls2;
            RLS3_A = rls3;
            RLS4_A = rls4;
            RLS5_P = rls5;
            targetCoords = ComputeTargetCoords();
        }

        public bool PossibilityGetTargetCoords()
        {
            return RLS5_P.getDistance() != 0
                && (RLS1_A.getDistance() != 0
                    || RLS2_A.getDistance() != 0
                    || RLS3_A.getDistance() != 0
                    || RLS4_A.getDistance() != 0);
        }

        private TargetCoords ComputeFromActiveRLS(RLS_Active activeRLS)
        {
            float sumAnglePassive = 0f;
            float sumAngleActive = 0f;
            float sumDistance = 0f;

            for (int i = 0; i < SampleCount; i++)
            {
                sumAnglePassive += RLS5_P.getAngleRadians();
                sumAngleActive += activeRLS.getAngleRadians();
                sumDistance += activeRLS.getDistance();
            }

            float passiveAngle = sumAnglePassive / SampleCount;
            float activeAngle = sumAngleActive / SampleCount;
            float activeDistance = sumDistance / SampleCount;

            tan_1 = Mathf.Tan(passiveAngle);
            tan_2 = Mathf.Tan(activeAngle);

            var coords = new TargetCoords();
            coords.z = (
                -tan_1 / tan_2 * RLS5_P.RLSPos.position.z
                + 1 / tan_2 * (RLS5_P.RLSPos.position.x - activeRLS.RLSPos.position.x)
                + activeRLS.RLSPos.position.z
            ) / (1 - tan_1 / tan_2);

            coords.x = (coords.z - activeRLS.RLSPos.position.z) * tan_2 + activeRLS.RLSPos.position.x;

            double dx = coords.x - activeRLS.RLSPos.position.x;
            double dz = coords.z - activeRLS.RLSPos.position.z;
            double lateralDistSq = Math.Pow(activeDistance, 2) - Math.Pow(dx, 2) - Math.Pow(dz, 2);
            coords.yAbove = Math.Sqrt(lateralDistSq) + activeRLS.RLSPos.position.y;

            return coords;
        }

        private TargetCoords ComputeTargetCoords()
        {
            if (!PossibilityGetTargetCoords())
                return new TargetCoords();

            RLS_Active activeRLS = RLS1_A.getDistance() != 0 ? RLS1_A
                : RLS2_A.getDistance() != 0 ? RLS2_A
                : RLS3_A.getDistance() != 0 ? RLS3_A
                : RLS4_A;

            TargetCoords coords = ComputeFromActiveRLS(activeRLS);

            float sumPassiveDist = 0f;
            for (int i = 0; i < SampleCount; i++)
                sumPassiveDist += RLS5_P.getDistance();

            float passiveDistance = sumPassiveDist / SampleCount;
            double dxP = coords.x - RLS5_P.RLSPos.position.x;
            double dzP = coords.z - RLS5_P.RLSPos.position.z;
            double passiveLateralSq = Math.Pow(passiveDistance, 2) - Math.Pow(dxP, 2) - Math.Pow(dzP, 2);
            double yAbovePassive = Math.Sqrt(passiveLateralSq) + RLS5_P.RLSPos.position.y;

            coords.y = (coords.yAbove + yAbovePassive) / 2;
            return coords;
        }

        public double getTargetX() => targetCoords.x;
        public double getTargetY() => targetCoords.y;
        public double getTargetZ() => targetCoords.z;
    }

    void Update()
    {
        var rls1 = new RLS_Active(RLS1, Drone, -90, 0, accuracyDistanceActive, accuracyAngleActive);
        var rls2 = new RLS_Active(RLS2, Drone, -180, -90, accuracyDistanceActive, accuracyAngleActive);
        var rls3 = new RLS_Active(RLS3, Drone, 90, 180, accuracyDistanceActive, accuracyAngleActive);
        var rls4 = new RLS_Active(RLS4, Drone, 0, 90, accuracyDistanceActive, accuracyAngleActive);
        var rls5 = new RLS_Passive(RLS5, Drone, accuracyDistancePassive, accuracyAnglePassive);

        var nav = new NavigationSystem(rls1, rls2, rls3, rls4, rls5);

        Target_X = nav.getTargetX();
        Target_Y = nav.getTargetY();
        Target_Z = nav.getTargetZ();

        Nav_Error = Math.Sqrt(
            Math.Pow(Target_X - Drone.position.x, 2) +
            Math.Pow(Target_Y - Drone.position.y, 2) +
            Math.Pow(Target_Z - Drone.position.z, 2)
        );
    }
}
