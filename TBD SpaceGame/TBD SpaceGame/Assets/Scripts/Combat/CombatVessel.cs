using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// Minimal representation of a vessel for combat-related systems.
    /// Stores thermal and sensor properties.
    /// </summary>
    public class CombatVessel : MonoBehaviour
    {
        // Engine settings
        public bool mainEngineActive;
        [Range(0f, 1f)] public float mainEngineThrustPercentage;
        public AnimationCurve heatGenerationCurve = AnimationCurve.Linear(0,0,1,1);
        public float mainEngineHeatOutput = 100f;

        // Reaction control system
        [Range(0f, 1f)] public float rcsActivityLevel;
        public float rcsHeatOutput = 5f;

        // Weapon systems
        public List<WeaponSystem> weaponSystems = new();

        // Baseline heat generation from life support/electronics
        public float baselineHeatGeneration = 10f;

        // Radiator properties
        public float radiatorCapacity = 50f;
        [Range(0f, 1f)] public float radiatorDeploymentPercentage = 1f;

        // Thermal state
        public float currentHeat = 0f;
        public float maximumSafeTemperature = 300f;
        public float infraredSignature = 0f;

        // Sensor properties
        public float baseSensorEfficiency = 1f;
        public float sensorRange = 1000f;

        // Environment
        public Vector3 sunDirection = Vector3.forward;
        public string currentRegion = "";
        public float currentSolarActivity = 0f;

        public WeaponSystem GetSystemOperator(ShipSystem system)
        {
            // Placeholder for retrieving crew/weapon operator
            return null;
        }
    }

    public enum ShipSystem
    {
        Sensors,
        Weapons,
        Engines
    }
}
