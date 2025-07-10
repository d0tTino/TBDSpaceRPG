using UnityEngine;

namespace OrbitalMechanics
{
    /// <summary>
    /// Placeholder structure for basic orbit parameters.
    /// A proper implementation will integrate with a Kepler orbit plugin.
    /// </summary>
    [System.Serializable]
    public class OrbitData
    {
        public Vector3d semiMajorAxis = new Vector3d(0, 0, 0);
        public double eccentricity = 0;
        public double inclination = 0;
        public double argumentOfPeriapsis = 0;
        public double longitudeOfAscendingNode = 0;
        public double meanAnomalyAtEpoch = 0;

        // Utility method placeholder
        public Vector3 GetPositionAtTime(double time)
        {
            // Placeholder returns position at epoch
            return Vector3.zero;
        }
    }
}
