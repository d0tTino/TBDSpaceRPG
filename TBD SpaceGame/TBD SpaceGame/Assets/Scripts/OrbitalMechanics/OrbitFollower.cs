using UnityEngine;

namespace OrbitalMechanics
{
    /// <summary>
    /// Follows a predefined orbit using data from OrbitData.
    /// </summary>
    [RequireComponent(typeof(SpaceshipMovement))]
    public class OrbitFollower : MonoBehaviour
    {
        private OrbitData _orbit;

        public void SetOrbit(OrbitData orbit)
        {
            _orbit = orbit;
        }

        private void FixedUpdate()
        {
            if (_orbit == null) return;
            // Placeholder: update transform along orbit
        }
    }
}
