using UnityEngine;
using OrbitalMechanics;

namespace Combat
{
    /// <summary>
    /// Generates orbital debris from ship combat impacts. This placeholder
    /// implementation creates simple rigidbody objects that drift away from
    /// the impact point.
    /// </summary>
    public class DebrisFieldGenerator : MonoBehaviour
    {
        [Header("Debris Settings")]
        public int minDebrisCount = 5;
        public int maxDebrisCount = 50;
        public float debrisGenerationFactor = 10f;
        public float minEjectionSpeed = 1f;
        public float ejectionSpeedFactor = 1f;
        public float minDebrisSize = 0.1f;
        public float maxDebrisSize = 2f;
        public float debrisDensity = 1f;

        /// <summary>
        /// Spawn debris objects based on impact force and target mass.
        /// </summary>
        public void GenerateCombatDebris(Vector3 position, float impactForce, float targetMass, OrbitData referenceOrbit)
        {
            int debrisCount = Mathf.FloorToInt(Mathf.Sqrt(impactForce * targetMass) / debrisGenerationFactor);
            debrisCount = Mathf.Clamp(debrisCount, minDebrisCount, maxDebrisCount);

            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                float speed = Random.Range(minEjectionSpeed, Mathf.Sqrt(impactForce / Mathf.Max(targetMass, 0.01f)) * ejectionSpeedFactor);
                Vector3 velocity = direction * speed;
                float size = Random.Range(minDebrisSize, maxDebrisSize);
                float mass = size * size * debrisDensity;

                GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debris.transform.position = position;
                debris.transform.localScale = Vector3.one * size;
                Rigidbody rb = debris.AddComponent<Rigidbody>();
                rb.mass = mass;
                rb.velocity = velocity;

                float lifetime = 30f;
                if (DebrisManager.Instance != null)
                {
                    DebrisManager.Instance.RegisterDebris(debris, lifetime);
                }
                else
                {
                    Destroy(debris, lifetime);
                }
            }
        }

        /// <summary>
        /// Estimates the chance of colliding with orbital debris during a transit.
        /// This simplified implementation scales with time and cross-sectional area.
        /// </summary>
        public float CalculateDebrisCollisionProbability(OrbitData shipOrbit, float transitTime, float shipCrossSectionalArea)
        {
            return Mathf.Clamp01(0.001f * shipCrossSectionalArea * transitTime);
        }
    }
}
