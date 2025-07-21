using System.Collections.Generic;

using Godot;

namespace OrbitalMechanics
{
    /// <summary>
    /// Manages all celestial bodies and provides a central lookup for orbital mechanics.
    /// Acts as a simple singleton for now.
    /// </summary>
    public class OrbitManager : Node
    {
        public static OrbitManager Instance { get; private set; }

        private readonly List<CelestialBody> bodies = new();
        public IReadOnlyList<CelestialBody> Bodies => bodies;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public void RegisterBody(CelestialBody body)
        {
            if (!bodies.Contains(body))
            {
                bodies.Add(body);
            }
        }

        public void UnregisterBody(CelestialBody body)
        {
            bodies.Remove(body);
        }

        public CelestialBody GetBody(string bodyName)
        {
            return bodies.Find(b => b.Name == bodyName);
        }
    }
}
