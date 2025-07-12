using System.Collections.Generic;

using UnityEngine;

namespace OrbitalMechanics
{
    /// <summary>
    /// Manages all celestial bodies and provides a central lookup for orbital mechanics.
    /// Acts as a simple singleton for now.
    /// </summary>
    public class OrbitManager : MonoBehaviour
    {
        public static OrbitManager Instance { get; private set; }

        private readonly List<CelestialBody> bodies = new();
        public IReadOnlyList<CelestialBody> Bodies => bodies;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
            return bodies.Find(b => b.name == bodyName);
        }
    }
}
