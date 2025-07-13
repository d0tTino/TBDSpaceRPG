using SimpleKeplerOrbits;

using UnityEngine;

namespace OrbitalMechanics
{
    /// <summary>
    /// Orbit data backed by the Simple Kepler Orbits plugin.
    /// </summary>
    [System.Serializable]
    public class OrbitData
    {
        public KeplerOrbit orbit = new KeplerOrbit();

        public Vector3 GetPositionAtTime(double time)
        {
            return (Vector3)orbit.GetRelativePositionAtTime(time);
        }
    }
}
