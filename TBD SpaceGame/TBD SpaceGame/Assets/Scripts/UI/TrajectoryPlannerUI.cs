using UnityEngine;
using OrbitalMechanics;

namespace UI
{
    /// <summary>
    /// Simple UI component for plotting orbital transfers between celestial bodies.
    /// The actual UI elements will be created within Unity's editor.
    /// </summary>
    public class TrajectoryPlannerUI : MonoBehaviour
    {
        public void OpenPlanner(CelestialBody origin, CelestialBody destination)
        {
            // Placeholder: display UI and call OrbitPlanner
            var planner = new OrbitPlanner();
            OrbitData orbit = planner.PlanTransfer(origin, destination);
            Debug.Log($"Planned transfer orbit from {origin.name} to {destination.name}");
        }
    }
}
