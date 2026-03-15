// Minimal Unity type stubs for running navigation math tests without the Unity engine.
// Only the subset used by the navigation and swarm logic is defined here.

using System;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x, y, z;

        public static readonly Vector3 zero = new Vector3(0, 0, 0);

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float dz = a.z - b.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static class Mathf
    {
        public const float Rad2Deg = 57.29578f;
        public const float Deg2Rad = 0.01745329f;
        public const float PI = 3.14159274f;

        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Sqrt(float f) => (float)Math.Sqrt(f);
        public static float Pow(float f, float p) => (float)Math.Pow(f, p);
        public static float Tan(float f) => (float)Math.Tan(f);
        public static float Abs(float f) => Math.Abs(f);
        public static float Pow(float f, int p) => (float)Math.Pow(f, p);
    }

    public static class Debug
    {
        public static void Log(object message) { }
    }

    public static class Random
    {
        // In tests we want deterministic results: return 0 so accuracy noise is zeroed out
        public static float Range(float min, float max) => 0f;
    }

    public class Transform
    {
        public Vector3 position { get; set; }

        public Transform(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
        }
    }

    public class MonoBehaviour { }
}
