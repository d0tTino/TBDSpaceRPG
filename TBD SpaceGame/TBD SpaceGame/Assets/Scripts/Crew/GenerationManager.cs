using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Manages long-term time progression and generational events.
    /// </summary>
    public class GenerationManager : MonoBehaviour
    {
        /// <summary>
        /// Fired when a time skip is requested.
        /// </summary>
        public event Action<TimeSkipRequest> OnTimeSkipRequested;

        private TimeSkipRequest pendingRequest;

        /// <summary>
        /// Creates a new <see cref="TimeSkipRequest"/> and notifies listeners.
        /// </summary>
        /// <param name="duration">Amount of game time to skip.</param>
        /// <param name="conditions">Interrupt conditions for the skip.</param>
        public void RequestTimeSkip(TimeSpan duration, TimeSkipConditions conditions)
        {
            pendingRequest = new TimeSkipRequest
            {
                Duration = duration,
                TargetDate = DateTime.Now.Add(duration),
                Conditions = conditions,
                InterruptConditionsMet = false,
                MilestoneEvents = new List<TimelineMilestone>()
            };

            // Notify any subscribers about the requested skip
            OnTimeSkipRequested?.Invoke(pendingRequest);

        }
    }
}
