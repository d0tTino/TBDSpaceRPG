using Godot;

namespace OrbitalMechanics
{
    /// <summary>
    /// Represents a celestial body with mass and initial orbit data.
    /// This component is intended to work with a future orbital mechanics plugin.
    /// </summary>
    public class CelestialBody : Node3D
    {
        [Export] private double mass = 1e12;
        [Export] private OrbitData initialOrbit;

        public double Mass => mass;
        public OrbitData InitialOrbit => initialOrbit;

        public override void _Ready()
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

        public override void _ExitTree()
        {
            if (OrbitManager.Instance != null)
            {
                OrbitManager.Instance.UnregisterBody(this);
            }
        }
    }
}
