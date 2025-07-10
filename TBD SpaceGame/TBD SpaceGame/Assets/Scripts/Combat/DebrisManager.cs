using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Tracks active debris objects so other systems can query hazard levels.
    /// Debris objects are automatically removed after their lifetime expires.
    /// </summary>
    public class DebrisManager : MonoBehaviour
    {
        public static DebrisManager Instance { get; private set; }

        private readonly List<GameObject> _activeDebris = new();
        public IReadOnlyList<GameObject> ActiveDebris => _activeDebris;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterDebris(GameObject debris, float lifetime)
        {
            _activeDebris.Add(debris);
            StartCoroutine(RemoveAfterLifetime(debris, lifetime));
        }

        private System.Collections.IEnumerator RemoveAfterLifetime(GameObject debris, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            _activeDebris.Remove(debris);
            if (debris != null)
            {
                Destroy(debris);
            }
        }
    }
}
