using Godot;

using SimpleKeplerOrbits;

namespace OrbitalMechanics
{
    /// <summary>
    /// Plans transfer orbits between celestial bodies.
    /// This is a minimal placeholder that will be expanded with real orbital calculations.
    /// </summary>
    public class OrbitPlanner
    {
        public OrbitData PlanTransfer(CelestialBody origin, CelestialBody destination)
        {
            var transferOrbit = new KeplerOrbit
            {
                semiMajorAxis = (origin.InitialOrbit.orbit.semiMajorAxis + destination.InitialOrbit.orbit.semiMajorAxis) * 0.5,
                eccentricity = 0.1,
                inclination = 0,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                meanAnomalyAtEpoch = 0,
                gravitationalParameter = origin.InitialOrbit.orbit.gravitationalParameter
            };

            return new OrbitData { orbit = transferOrbit };
        }

        /// <summary>
        /// Creates a simple circular orbit around the specified body.
        /// </summary>
        public OrbitData PlanCircularOrbit(CelestialBody body, double radius)
        {
            var orbit = new KeplerOrbit
            {
                semiMajorAxis = radius,
                eccentricity = 0,
                inclination = 0,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                meanAnomalyAtEpoch = 0,
                gravitationalParameter = body.InitialOrbit.orbit.gravitationalParameter
            };

            return new OrbitData { orbit = orbit };
        }
    }
}
