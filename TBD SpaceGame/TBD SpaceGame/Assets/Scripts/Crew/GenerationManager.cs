using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Handles generational progression and exposes events for births and natural deaths.
    /// This is a minimal placeholder so other systems can react to population changes.
    /// </summary>
    public class GenerationManager : MonoBehaviour
    {
        public event Action<CrewMember> OnCrewBorn;
        public event Action<CrewMember> OnCrewDied;

        /// <summary>
        /// Invoke when a new crew member is born.
        /// </summary>
        public void Birth(CrewMember member)
        {
            OnCrewBorn?.Invoke(member);
        }

        /// <summary>
        /// Invoke when a crew member dies of natural causes.
        /// </summary>
        public void NaturalDeath(CrewMember member)
        {
            OnCrewDied?.Invoke(member);
        }

        /// <summary>
        /// Simulate a time skip by processing births and deaths.
        /// </summary>
        public void ProcessTimeSkip(List<CrewMember> births, List<CrewMember> deaths)
        {
            foreach (var birth in births)
            {
                Birth(birth);
            }

            foreach (var death in deaths)
            {
                NaturalDeath(death);
            }

        }
    }
}
