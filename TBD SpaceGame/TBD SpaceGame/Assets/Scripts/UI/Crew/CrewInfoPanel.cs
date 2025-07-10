using UnityEngine;
using Crew;

namespace UI.Crew
{
    /// <summary>
    /// Displays basic information about a single crew member.
    /// </summary>
    public class CrewInfoPanel : MonoBehaviour
    {
        private CrewMember _member;

        /// <summary>
        /// Initialize panel with a crew member instance.
        /// </summary>
        public void Initialize(CrewMember member)
        {
            _member = member;
            Refresh();

            if (GenerationManager.Instance != null)
            {
                GenerationManager.Instance.OnGenerationAdvanced += Refresh;
            }
        }

        /// <summary>
        /// Refresh displayed information.
        /// </summary>
        public void Refresh()
        {
            if (_member == null) return;
            // Actual UI elements would be updated here in a full implementation.
            Debug.Log($"Refreshing info for {_member.role}");
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
