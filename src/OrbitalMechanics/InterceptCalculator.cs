using System.Collections.Generic;

using Godot;

namespace OrbitalMechanics.Combat
{
    /// <summary>
    /// Provides basic intercept calculation methods for projectiles following orbital trajectories.
    /// This is a simplified placeholder based on the Technical Implementation document.
    /// </summary>
    public class InterceptCalculator
    {
        private const int INITIAL_SEARCH_STEPS = 10;
        private const float COARSE_SEARCH_THRESHOLD = 5f;
        private const int MAX_REFINEMENT_ITERATIONS = 10;
        private const float CONVERGENCE_THRESHOLD = 0.1f;

        public InterceptSolution? CalculateOrbitalIntercept(
            Vector3 firingPosition,
            Vector3 firingVelocity,
            OrbitData targetOrbit,
            float projectileSpeed,
            float maxTimeToIntercept)
        {
            var potentialTimes = new List<float>();
            float timeStep = maxTimeToIntercept / INITIAL_SEARCH_STEPS;

            for (float t = 0f; t <= maxTimeToIntercept; t += timeStep)
            {
                Vector3 targetPos = targetOrbit.GetPositionAtTime(t);
                float distance = Vector3.Distance(firingPosition, targetPos);
                float travelTime = distance / projectileSpeed;

                if (Mathf.Abs(travelTime - t) < COARSE_SEARCH_THRESHOLD)
                {
                    potentialTimes.Add(t);
                }
            }

            InterceptSolution? bestSolution = null;
            float lowestDeltaV = float.MaxValue;

            foreach (float guess in potentialTimes)
            {
                InterceptSolution? solution = RefineInterceptSolution(
                    firingPosition,
                    firingVelocity,
                    targetOrbit,
                    projectileSpeed,
                    guess);

                if (solution != null)
                {
                    float requiredDeltaV = (solution.Value.FiringVector - firingVelocity).magnitude;

                    if (requiredDeltaV < lowestDeltaV)
                    {
                        lowestDeltaV = requiredDeltaV;
                        bestSolution = solution;
                    }
                }
            }

            return bestSolution;
        }

        private InterceptSolution? RefineInterceptSolution(
            Vector3 firingPosition,
            Vector3 firingVelocity,
            OrbitData targetOrbit,
            float projectileSpeed,
            float initialTimeGuess)
        {
            float timeGuess = initialTimeGuess;
            float previousError = float.MaxValue;

            for (int i = 0; i < MAX_REFINEMENT_ITERATIONS; i++)
            {
                Vector3 targetPosition = targetOrbit.GetPositionAtTime(timeGuess);
                float distance = Vector3.Distance(firingPosition, targetPosition);
                float travelTime = distance / projectileSpeed;

                float error = travelTime - timeGuess;

                if (Mathf.Abs(error) < CONVERGENCE_THRESHOLD)
                {
                    Vector3 firingDirection = (targetPosition - firingPosition).normalized;
                    Vector3 firingVector = firingDirection * projectileSpeed;

                    return new InterceptSolution
                    {
                        FiringVector = firingVector,
                        TimeToIntercept = travelTime,
                        TargetPosition = targetPosition
                    };
                }

                if (Mathf.Abs(error) > Mathf.Abs(previousError))
                {
                    break;
                }

                float derivativeApprox = -1.2f;
                timeGuess = timeGuess - error / derivativeApprox;
                previousError = error;
            }

            return null;
        }
    }

    public struct InterceptSolution
    {
        public Vector3 FiringVector;
        public float TimeToIntercept;
        public Vector3 TargetPosition;
    }
}
