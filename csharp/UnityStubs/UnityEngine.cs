using System;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float dz = a.z - b.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public Vector3 normalized
        {
            get
            {
                float mag = (float)Math.Sqrt(x * x + y * y + z * z);
                return mag > 1e-5 ? new Vector3(x / mag, y / mag, z / mag) : zero;
            }
        }
        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 v, float d) => new Vector3(v.x * d, v.y * d, v.z * d);

    }

    public class MonoBehaviour
    {
        public Transform transform = new Transform();
        public GameObject gameObject = new GameObject();
        public string name { get => gameObject.name; set => gameObject.name = value; }
        public static void Destroy(object obj) { }

    }

    public class SerializeField : Attribute { }
    public class RequireComponent : Attribute { public RequireComponent(Type t) { } }

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
        public Vector3 forward = Vector3.forward;
    }

    public class GameObject
    {
        public string name = string.Empty;
        public Transform transform = new Transform();
        private readonly System.Collections.Generic.Dictionary<System.Type, object> _components = new System.Collections.Generic.Dictionary<System.Type, object>();
        public T AddComponent<T>() where T : new()
        {
            var comp = new T();
            _components[typeof(T)] = comp;
            return comp;
        }
        public T GetComponent<T>() where T : class
        {
            _components.TryGetValue(typeof(T), out var obj);
            return obj as T;
        }
    }

    public static class Debug
    {
        public static void Log(string msg) { }
        public static void LogError(string msg) { }
    }

    public enum KeyCode { W, A, S, D }

    public static class Input
    {
        private static readonly System.Collections.Generic.HashSet<KeyCode> _pressed = new System.Collections.Generic.HashSet<KeyCode>();
        public static bool GetKey(KeyCode code) => _pressed.Contains(code);
        public static void SetKey(KeyCode code, bool value)
        {
            if (value) _pressed.Add(code); else _pressed.Remove(code);
        }
    }

    public class Rigidbody
    {
        public Vector3 velocity;
        public Vector3 torque;
        public void AddForce(Vector3 force) { velocity += force; }
        public void AddTorque(Vector3 t) { torque += t; }
    }

}
public class SpaceshipMovement : UnityEngine.MonoBehaviour { }

