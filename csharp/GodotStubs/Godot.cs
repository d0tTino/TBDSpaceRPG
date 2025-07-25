using System;
using System.Collections.Generic;

namespace Godot
{
    public class ExportAttribute : Attribute { }
    public class ToolAttribute : Attribute { }
    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Forward => new Vector3(0, 0, -1);
        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public Vector3 Normalized => Length() > 1e-5 ? this * (1 / Length()) : Zero;
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 v, float d) => new Vector3(v.X * d, v.Y * d, v.Z * d);
        public static Vector3 operator *(float d, Vector3 v) => v * d;
        public static float Distance(Vector3 a, Vector3 b) => (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
        public float magnitude => Length();
        public Vector3 normalized => Normalized;
        public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);
        public Vector3 LimitLength(float max) => Length() > max ? Normalized * max : this;
        public static Vector3 Clamp(Vector3 v, float max) => v.Length() > max ? v.Normalized * max : v;
    }

    public static class Mathf
    {
        public static float DegToRad(float deg) => (float)(Math.PI / 180) * deg;
        public static float Abs(float f) => Math.Abs(f);
    }

    public class Node
    {
        public List<Node> Children { get; } = new();
        public string Name { get; set; } = string.Empty;
        public Node? Parent { get; private set; }
        public virtual void AddChild(Node node) { node.Parent = this; Children.Add(node); }
        public void AddToGroup(string group) { }
        public virtual void QueueFree() { }
        public Node? GetParent() => Parent;
        public T? GetNodeOrNull<T>(string path) where T : class => null;
        public T? GetNode<T>(string path) where T : class => null;
        public Node? FindChild(string name, bool recursive, bool owned) => null;
        public SceneTree GetTree() => new SceneTree { Root = this };
        public virtual void _Ready() { }
        public virtual void _ExitTree() { }
    }

    public class SceneTree : Node
    {
        public Node Root { get; set; } = new Node();
    }

    public class Control : Node { }
    public class Button : Control { public event Action? Pressed; public void EmitPressed() => Pressed?.Invoke(); }
    public class CheckButton : Button { public event Action<bool>? Toggled; public bool ToggleValue; public void EmitToggled(bool v) { ToggleValue = v; Toggled?.Invoke(v); } }
    public class Label : Control { public string Text { get; set; } = string.Empty; public bool Visible { get; set; } = true; }

    public class Node3D : Node
    {
        public Vector3 GlobalPosition;
        public Transform3D Transform { get; } = new Transform3D();
        public void RotateY(float angle) { }
        public virtual void _PhysicsProcess(double delta) { }
    }

    public class CharacterBody3D : Node3D
    {
        public Vector3 Velocity;
        public void MoveAndSlide() { }
        public override void _PhysicsProcess(double delta) { }
    }

    public class Transform3D
    {
        public Basis Basis { get; } = new Basis();
    }

    public class Basis
    {
        public Vector3 Z = Vector3.Forward;
    }

    public static class Time { public static double GetTicksMsec() => 0; public static float time; }
    public static class Input
    {
        private static readonly HashSet<string> _pressed = new();
        public static bool IsActionPressed(string action) => _pressed.Contains(action);
        public static void ActionPress(string action) { _pressed.Add(action); }
        public static void ActionRelease(string action) { _pressed.Remove(action); }
    }

    public class EditorPlugin : Node { }
    public class EditorInterface { }

    public static class GD
    {
        public static void Print(string msg) { }
        public static void PrintErr(string msg) { }
    }

    public enum Error { Ok }

    public class HttpClient
    {
        public enum Method { Get, Post }
    }

    public partial class HttpRequest : Node
    {
        public static class SignalName
        {
            public const string RequestCompleted = "request_completed";
        }

        public Error Request(string url, string[] headers, HttpClient.Method method, string body) => Error.Ok;
        public int GetResponseCode() => 200;
    }

    namespace Collections
    {
        public class Array : List<object> { }
    }

    public class Variant
    {
        public enum Type { String, StringName }
        public Type VariantType { get; set; }
        public object? Value { get; set; }
        public static implicit operator string?(Variant v) => v.Value as string;
        public static implicit operator StringName?(Variant v) => v.Value as StringName;
    }

    public class StringName
    {
        private readonly string _value;
        public StringName(string value) { _value = value; }
        public override string ToString() => _value;
    }

    public static class ProjectSettings
    {
        public static bool HasSetting(string name) => false;
        public static Variant GetSetting(string name) => new Variant();
    }

    public static class GodotTaskExtensions
    {
        public static System.Threading.Tasks.Task<Variant[]> ToSignal(this Node node, string signal)
            => System.Threading.Tasks.Task.FromResult(new Variant[0]);
    }
}
