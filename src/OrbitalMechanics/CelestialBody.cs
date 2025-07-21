using UnityEngine;

namespace OrbitalMechanics
{
    /// <summary>
    /// Represents a celestial body with mass and initial orbit data.
    /// This component is intended to work with a future orbital mechanics plugin.
    /// </summary>
    public class CelestialBody : MonoBehaviour
    {
        [SerializeField] private double mass = 1e12;
        [SerializeField] private OrbitData initialOrbit;

        public double Mass => mass;
        public OrbitData InitialOrbit => initialOrbit;

        private void Awake()
        {
            // Ensure we always have valid orbit data
            if (initialOrbit == null)
            {
                initialOrbit = new OrbitData();
            }

            // Register this body with the global orbit manager if present
            if (OrbitManager.Instance != null)
            {
                OrbitManager.Instance.RegisterBody(this);
            }
        }

        private void OnDestroy()
        {
            if (OrbitManager.Instance != null)
            {
                OrbitManager.Instance.UnregisterBody(this);
            }
        }
    }
}
