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
    }
}
