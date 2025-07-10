using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Handles heat generation, dissipation and detection probability calculations.
    /// Placeholder implementation based on the technical design document.
    /// </summary>
    public static class ThermalSignatureSystem
    {
        private const float AMBIENT_TEMPERATURE = 3f; // K, space background
        private const float OVERHEAT_DAMAGE_RATE = 5f;
        private const float STANDARD_THERMAL_SIGNATURE = 100f;

        public static void UpdateThermalSignature(CombatVessel vessel, float deltaTime)
        {
            float heatGeneration = 0f;

            if (vessel.mainEngineActive)
            {
                heatGeneration += vessel.mainEngineThrustPercentage *
                                   vessel.heatGenerationCurve.Evaluate(vessel.mainEngineThrustPercentage) *
                                   vessel.mainEngineHeatOutput;
            }

            heatGeneration += vessel.rcsActivityLevel * vessel.rcsHeatOutput;

            foreach (var weapon in vessel.weaponSystems)
            {
                if (weapon != null && weapon.isActive)
                {
                    heatGeneration += weapon.currentHeatGeneration;
                }
            }

            heatGeneration += vessel.baselineHeatGeneration;

            float radiatorEfficiency = CalculateRadiatorEfficiency(
                vessel.transform.rotation,
                vessel.sunDirection);

            float heatDissipation = vessel.radiatorCapacity *
                                    radiatorEfficiency *
                                    vessel.radiatorDeploymentPercentage;

            float netHeatChange = heatGeneration - heatDissipation;

            vessel.currentHeat += netHeatChange * deltaTime;
            vessel.currentHeat = Mathf.Clamp(
                vessel.currentHeat,
                AMBIENT_TEMPERATURE,
                vessel.maximumSafeTemperature * 1.5f);

            vessel.infraredSignature = CalculateInfraredSignature(
                vessel.currentHeat,
                1f, // placeholder hull area
                0.8f); // placeholder emissivity

            if (vessel.currentHeat > vessel.maximumSafeTemperature)
            {
                float overHeatRatio = (vessel.currentHeat - vessel.maximumSafeTemperature) /
                                      (vessel.maximumSafeTemperature * 0.5f);
                ApplyOverheatDamage(vessel, overHeatRatio * OVERHEAT_DAMAGE_RATE * deltaTime);
            }
        }

        public static float CalculateDetectionProbability(
            CombatVessel target,
            CombatVessel observer,
            float distance)
        {
            float baseProbability = 1.0f / (1.0f + (distance * distance / (observer.sensorRange * observer.sensorRange)));
            float signatureMultiplier = target.infraredSignature / STANDARD_THERMAL_SIGNATURE;
            float sensorEfficiency = observer.baseSensorEfficiency;
            // Crew skill placeholder omitted
            float detectionProbability = baseProbability * signatureMultiplier * sensorEfficiency;
            detectionProbability *= CalculateEnvironmentalDetectionModifier(observer.currentRegion, observer.currentSolarActivity);
            return Mathf.Clamp01(detectionProbability);
        }

        private static float CalculateRadiatorEfficiency(Quaternion vesselRotation, Vector3 sunDirection)
        {
            // Placeholder simple cosine falloff
            return Mathf.Clamp01(Vector3.Dot(vesselRotation * Vector3.forward, sunDirection.normalized));
        }

        private static float CalculateInfraredSignature(float heat, float hullArea, float emissivity)
        {
            // Placeholder using Stefan-Boltzmann-like approximation
            return heat * hullArea * emissivity;
        }

        private static float CalculateEnvironmentalDetectionModifier(string region, float solarActivity)
        {
            // Placeholder constant factor
            return 1.0f - Mathf.Clamp01(solarActivity);
        }

        private static void ApplyOverheatDamage(CombatVessel vessel, float damageAmount)
        {
            // Placeholder for damage logic
            Debug.Log($"{vessel.name} takes {damageAmount} thermal damage");
        }
    }
}
