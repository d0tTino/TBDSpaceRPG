using System;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float dz = a.z - b.z;
            return (float)Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }
        public Vector3 normalized
        {
            get
            {
                float mag = (float)Math.Sqrt(x*x + y*y + z*z);
                return mag > 1e-5 ? new Vector3(x/mag, y/mag, z/mag) : zero;
            }
        }
        public float magnitude => (float)Math.Sqrt(x*x + y*y + z*z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x-b.x, a.y-b.y, a.z-b.z);
        public static Vector3 operator *(Vector3 v, float d) => new Vector3(v.x*d, v.y*d, v.z*d);
    }

    public class MonoBehaviour {
        public Transform transform = new Transform();
        public GameObject gameObject = new GameObject();
        public string name { get => gameObject.name; set => gameObject.name = value; }
        public static void Destroy(object obj) {}
    }

    public class SerializeField : Attribute {}
    public class RequireComponent : Attribute { public RequireComponent(Type t) {} }

    public static class Mathf
    {
        public static float Abs(float f) => Math.Abs(f);
    }

    public static class Time
    {
        public static float time;
    }

    public class Transform
    {
        public Vector3 position;
    }

    public class GameObject
    {
        public string name = string.Empty;
        public Transform transform = new Transform();
    }

}
public class SpaceshipMovement : UnityEngine.MonoBehaviour {}
