using System;
using System.Collections.Generic;

namespace Crew
{
    /// <summary>
    /// Describes a request to advance game time by a duration while monitoring
    /// optional interruption conditions.
    /// </summary>
    [Serializable]
    public struct TimeSkipRequest
    {
        /// <summary>
        /// Duration to skip forward.
        /// </summary>
        public TimeSpan Duration;

        /// <summary>
        /// The date the game should reach after the skip.
        /// </summary>
        public DateTime TargetDate;

        /// <summary>
        /// Conditions that can interrupt the skip.
        /// </summary>
        public TimeSkipConditions Conditions;

        /// <summary>
        /// Generated milestone events that occur during the skip.
        /// </summary>
        public List<TimelineMilestone> MilestoneEvents;

        /// <summary>
        /// Indicates if any interrupt conditions have been met.
        /// </summary>
        public bool InterruptConditionsMet;
    }

    /// <summary>
    /// Defines conditions that may interrupt a time skip.
    /// </summary>
    [Serializable]
    public class TimeSkipConditions
    {
        public bool InterruptOnCriticalEvents = true;
        public bool InterruptOnResourceShortage = true;
        public bool InterruptOnCrewConflict = true;
        public bool InterruptOnExternalContact = true;
        public bool InterruptOnSystemFailure = true;
        public bool InterruptOnResearchCompletion = true;

        /// <summary>
        /// Determines whether at least one interrupt condition is active.
        /// </summary>
        public bool HasAnyConditions()
        {
            return InterruptOnCriticalEvents || InterruptOnResourceShortage ||
                   InterruptOnCrewConflict || InterruptOnExternalContact ||
                   InterruptOnSystemFailure || InterruptOnResearchCompletion;
        }
    }

    /// <summary>
    /// Represents a significant event that occurs during a time skip.
    /// </summary>
    [Serializable]
    public class TimelineMilestone
    {
        public string ID;
        public string Title;
        public string Description;
        public DateTime OccurrenceDate;
        public bool RequiresPlayerIntervention;
    }
}
