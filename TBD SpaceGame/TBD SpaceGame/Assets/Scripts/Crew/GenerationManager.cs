using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Handles long duration time skips and generational changes for the crew.
    /// </summary>
    public class GenerationManager : MonoBehaviour
    {
        public event Action<TimeSpan> OnTimeSkipRequested;
        public event Action<int> OnGenerationAdvanced;
        public event Action OnTimeSkipCompleted;

        public CrewManager crewManager;

        private readonly List<int> populationHistory = new();
        private TimeSpan _pendingSkip;
        private int _currentGeneration = 1;

        /// <summary>
        /// Represents basic generational statistics.
        /// </summary>
        public struct GenerationStats
        {
            public int generation;
            public int population;
        }

        /// <summary>
        /// Get statistics for the current generation.
        /// </summary>
        public GenerationStats CurrentStats
        {
            get
            {
                return new GenerationStats
                {
                    generation = _currentGeneration,
                    population = crewManager != null ? crewManager.activeCrew.Count : 0
                };
            }
        }

        /// <summary>
        /// Request that the simulation advance by the given duration.
        /// </summary>
        public void RequestTimeSkip(TimeSpan duration)
        {
            _pendingSkip = duration;
            OnTimeSkipRequested?.Invoke(duration);
            ProcessTimeSkip();
        }

        /// <summary>
        /// Processes the previously requested time skip, ageing crew and spawning new generations.
        /// </summary>
        public void ProcessTimeSkip()
        {
            if (crewManager == null) return;

            double years = _pendingSkip.TotalDays / 365.0;
            int wholeYears = (int)Math.Floor(years);

            for (int i = crewManager.activeCrew.Count - 1; i >= 0; i--)
            {
                CrewMember member = crewManager.activeCrew[i];
                member.age += wholeYears;
                if (member.age >= 80)
                {
                    crewManager.Remove(member);
                }
            }

            for (int b = 0; b < wholeYears; b++)
            {
                crewManager.Recruit(new CrewMember { role = "Child", age = 0 });
            }

            populationHistory.Add(crewManager.activeCrew.Count);
            _currentGeneration++;
            OnGenerationAdvanced?.Invoke(_currentGeneration);
            OnTimeSkipCompleted?.Invoke();
        }

        /// <summary>
        /// Get the recorded population count for each completed generation.
        /// </summary>
        public IReadOnlyList<int> GetPopulationHistory()
        {
            return populationHistory.AsReadOnly();
        }
    }
}
