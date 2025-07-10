using System.Collections.Generic;
using UnityEngine;
using Crew;

namespace UI.Crew
{
    /// <summary>
    /// Displays a list of crew members and responds to generation changes.
    /// </summary>
    public class CrewListView : MonoBehaviour
    {
        private CrewManager _crewManager;

        /// <summary>
        /// Initialize the list view with a CrewManager instance.
        /// </summary>
        public void Initialize(CrewManager manager)
        {
            _crewManager = manager;
            Refresh();

            if (GenerationManager.Instance != null)
            {
                GenerationManager.Instance.OnGenerationAdvanced += Refresh;
            }
        }

        /// <summary>
        /// Refresh the displayed list.
        /// </summary>
        public void Refresh()
        {
            if (_crewManager == null) return;
            List<CrewMember> crew = _crewManager.activeCrew;
            // Actual UI elements would be updated here in a full implementation.
            Debug.Log($"Refreshing crew list with {crew.Count} members");
        }

        private void OnDestroy()
        {
            if (GenerationManager.Instance != null)
            {
                GenerationManager.Instance.OnGenerationAdvanced -= Refresh;
            }
        }
    }
}
