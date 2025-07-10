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
        [SerializeField] private GenerationManager generationManager;
        public List<CrewMember> activeCrew = new();

        private void Awake()
        {
            if (generationManager == null)
            {
                generationManager = FindObjectOfType<GenerationManager>();
            }

            if (generationManager != null)
            {
                generationManager.OnCrewBorn += OnCrewBorn;
                generationManager.OnCrewDied += OnCrewDied;
            }
        }

        private void OnDisable()
        {
            if (generationManager != null)
            {
                generationManager.OnCrewBorn -= OnCrewBorn;
                generationManager.OnCrewDied -= OnCrewDied;
            }
        }

        /// <summary>
        /// Add a crew member to the active roster.
        /// The optional <paramref name="fromBirth"/> flag is used when
        /// the member was born during a time skip.
        /// </summary>
        public void Recruit(CrewMember member, bool fromBirth = false)
        {
            if (member != null && !activeCrew.Contains(member))
            {
                activeCrew.Add(member);
                if (!fromBirth)
                {
                    generationManager?.Birth(member);
                }
            }
        }

        /// <summary>
        /// Remove a crew member from the active roster.
        /// The optional <paramref name="naturalDeath"/> flag indicates the
        /// removal is due to natural causes during a time skip.
        /// </summary>
        public void Remove(CrewMember member, bool naturalDeath = false)
        {
            if (member != null)
            {
                activeCrew.Remove(member);
                if (!naturalDeath)
                {
                    generationManager?.NaturalDeath(member);
                }
            }
        }

        private void OnCrewBorn(CrewMember member)
        {
            Recruit(member, true);
        }

        private void OnCrewDied(CrewMember member)
        {
            Remove(member, true);
        }

        /// <summary>
        /// Save the current crew roster to a JSON file.
        /// </summary>
        public void SaveToFile(string path)
        {
            var container = new CrewContainer { crew = activeCrew };
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
            activeCrew = container != null ? container.crew : new List<CrewMember>();
        }

        [System.Serializable]
        private class CrewContainer
        {
            public List<CrewMember> crew = new();

        }
    }
}
