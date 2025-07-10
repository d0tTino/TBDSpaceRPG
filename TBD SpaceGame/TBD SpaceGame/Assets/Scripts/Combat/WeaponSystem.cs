using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Represents a basic weapon that contributes to thermal signature.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        public bool isActive;
        public float currentHeatGeneration = 20f;
    }
}
