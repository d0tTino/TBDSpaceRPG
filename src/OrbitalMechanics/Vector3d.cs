using System;

using Godot;

namespace OrbitalMechanics
{
    /// <summary>
    /// Simple double precision vector used for orbital calculations.
    /// </summary>
    [Serializable]
    public struct Vector3d
    {
        public double x;
        public double y;
        public double z;

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
    }
}
