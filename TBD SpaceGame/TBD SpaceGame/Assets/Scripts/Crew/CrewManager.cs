using System.Collections.Generic;
using UnityEngine;

namespace Crew
{
    /// <summary>
    /// Central management for all crew members and their relationships.
    /// </summary>
    public class CrewManager : MonoBehaviour
    {
        public static CrewManager Instance { get; private set; }

        private readonly Dictionary<string, Relationship> _relationships = new();
        public IReadOnlyDictionary<string, Relationship> Relationships => _relationships;

        public event System.Action<Relationship> OnRelationshipChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Retrieve the relationship between two crew members, creating one if necessary.
        /// </summary>
        public Relationship GetRelationship(string idA, string idB)
        {
            string key = Relationship.GetKey(idA, idB);
            if (!_relationships.TryGetValue(key, out Relationship relationship))
            {
                relationship = new Relationship { CrewIdA = idA, CrewIdB = idB };
                _relationships[key] = relationship;
            }
            return relationship;
        }

        /// <summary>
        /// Modify the relationship between two crew members.
        /// </summary>
        public void ModifyRelationship(string idA, string idB, float affinityDelta, float trustDelta, float respectDelta)
        {
            Relationship relationship = GetRelationship(idA, idB);
            relationship.ApplyChange(affinityDelta, trustDelta, respectDelta);
            OnRelationshipChanged?.Invoke(relationship);
        }

        /// <summary>
        /// Apply time based updates to all relationships. Call periodically.
        /// </summary>
        public void UpdateRelationships(float deltaTime)
        {
            foreach (Relationship relationship in _relationships.Values)
            {
                relationship.UpdateOverTime(deltaTime);
            }
        }

        /// <summary>
        /// Hook for events from external systems that impact relationships.
        /// </summary>
        public void RegisterEventImpact(string idA, string idB, float affinityDelta, float trustDelta, float respectDelta)
        {
            ModifyRelationship(idA, idB, affinityDelta, trustDelta, respectDelta);
        }
    }
}
