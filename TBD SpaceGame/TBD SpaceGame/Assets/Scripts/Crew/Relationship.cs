using System;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Represents the relationship state between two crew members.
    /// </summary>
    [Serializable]
    public class Relationship
    {
        public string CrewIdA;
        public string CrewIdB;

        [Range(-100f, 100f)] public float Affinity;
        [Range(0f, 100f)] public float Trust;
        [Range(0f, 100f)] public float Respect;

        /// <summary>
        /// Apply continuous decay or improvement over time.
        /// </summary>
        public void UpdateOverTime(float deltaTime)
        {
            if (Affinity > 0f)
            {
                Affinity = Mathf.Max(0f, Affinity - deltaTime);
            }
            else if (Affinity < 0f)
            {
                Affinity = Mathf.Min(0f, Affinity + deltaTime);
            }
        }

        /// <summary>
        /// Modify relationship values based on an event.
        /// </summary>
        public void ApplyChange(float affinityDelta, float trustDelta, float respectDelta)
        {
            Affinity = Mathf.Clamp(Affinity + affinityDelta, -100f, 100f);
            Trust = Mathf.Clamp(Trust + trustDelta, 0f, 100f);
            Respect = Mathf.Clamp(Respect + respectDelta, 0f, 100f);
        }

        public static string GetKey(string idA, string idB)
        {
            return string.CompareOrdinal(idA, idB) <= 0 ? $"{idA}-{idB}" : $"{idB}-{idA}";
        }
    }
}
