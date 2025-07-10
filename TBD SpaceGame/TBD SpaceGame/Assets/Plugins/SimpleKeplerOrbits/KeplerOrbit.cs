using System;
using UnityEngine;
using OrbitalMechanics;

namespace SimpleKeplerOrbits
{
    [Serializable]
    public class KeplerOrbit
    {
        public double semiMajorAxis = 1;
        public double eccentricity = 0;
        public double inclination = 0;
        public double argumentOfPeriapsis = 0;
        public double longitudeOfAscendingNode = 0;
        public double meanAnomalyAtEpoch = 0;
        public double gravitationalParameter = 1;

        public Vector3d GetRelativePositionAtTime(double time)
        {
            double n = Math.Sqrt(gravitationalParameter / Math.Pow(semiMajorAxis, 3));
            double M = meanAnomalyAtEpoch + n * time;
            double E = SolveKepler(M, eccentricity);
            double cosE = Math.Cos(E);
            double sinE = Math.Sin(E);
            double r = semiMajorAxis * (1 - eccentricity * cosE);
            double xOrb = r * cosE - semiMajorAxis * eccentricity;
            double yOrb = r * sinE * Math.Sqrt(1 - eccentricity * eccentricity);

            double cosW = Math.Cos(argumentOfPeriapsis);
            double sinW = Math.Sin(argumentOfPeriapsis);
            double cosO = Math.Cos(longitudeOfAscendingNode);
            double sinO = Math.Sin(longitudeOfAscendingNode);
            double cosI = Math.Cos(inclination);
            double sinI = Math.Sin(inclination);

            double x = (cosO * cosW - sinO * sinW * cosI) * xOrb + (-cosO * sinW - sinO * cosW * cosI) * yOrb;
            double y = (sinO * cosW + cosO * sinW * cosI) * xOrb + (-sinO * sinW + cosO * cosW * cosI) * yOrb;
            double z = sinW * sinI * xOrb + cosW * sinI * yOrb;

            return new Vector3d(x, y, z);
        }

        private static double SolveKepler(double M, double e)
        {
            double E = M;
            for (int i = 0; i < 5; i++)
            {
                E = E - (E - e * Math.Sin(E) - M) / (1 - e * Math.Cos(E));
            }
            return E;
        }
    }
}
