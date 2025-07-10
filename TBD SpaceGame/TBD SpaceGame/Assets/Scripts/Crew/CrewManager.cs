using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Maintains the active crew roster and handles persistence.
    /// </summary>
    public class CrewManager : MonoBehaviour
    {
        public List<CrewMember> activeCrew = new();
        public int generation;
        public string lastSaveTimestamp = string.Empty;

        /// <summary>
        /// Add a crew member to the active roster.
        /// </summary>
        public void Recruit(CrewMember member)
        {
            if (member != null && !activeCrew.Contains(member))
            {
                activeCrew.Add(member);
            }
        }

        /// <summary>
        /// Remove a crew member from the active roster.
        /// </summary>
        public void Remove(CrewMember member)
        {
            if (member != null)
            {
                activeCrew.Remove(member);
            }
        }

        /// <summary>
        /// Save the current crew roster to a JSON file.
        /// Example structure:
        /// {
        ///   "crew": [ ... ],
        ///   "generation": 3,
        ///   "timestamp": "2024-01-01T00:00:00Z"
        /// }
        /// </summary>
        public void SaveToFile(string path)
        {
            lastSaveTimestamp = DateTime.UtcNow.ToString("o");
            var container = new CrewContainer
            {
                crew = activeCrew,
                generation = generation,
                timestamp = lastSaveTimestamp
            };
            string json = JsonUtility.ToJson(container, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Load the crew roster from a JSON file.
        /// </summary>
        public void LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Crew file not found at {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var container = JsonUtility.FromJson<CrewContainer>(json);
            if (container != null)
            {
                activeCrew = container.crew ?? new List<CrewMember>();
                generation = container.generation;
                lastSaveTimestamp = container.timestamp;
            }
            else
            {
                activeCrew = new List<CrewMember>();
            }
        }

        [System.Serializable]
        private class CrewContainer
        {
            public List<CrewMember> crew = new();
            public int generation;
            public string timestamp = string.Empty;

        }
    }
}
