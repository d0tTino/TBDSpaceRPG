using NUnit.Framework;
using OrbitalMechanics;
using OrbitalMechanics.Combat;
using SimpleKeplerOrbits;
using UnityEngine;
using System.Reflection;

namespace OrbitalMechanics.Tests
{
    public class OrbitTests
    {
        [Test]
        public void PlannerReturnsOrbit()
        {
            var origin = new CelestialBody();
            var dest = new CelestialBody();

            typeof(CelestialBody).GetField("initialOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(origin, new OrbitData { orbit = new KeplerOrbit { semiMajorAxis = 10, gravitationalParameter = 1 } });
            typeof(CelestialBody).GetField("initialOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(dest, new OrbitData { orbit = new KeplerOrbit { semiMajorAxis = 20, gravitationalParameter = 1 } });

            var planner = new OrbitPlanner();
            var result = planner.PlanTransfer(origin, dest);
            Assert.IsNotNull(result);
            Assert.Greater(result.orbit.semiMajorAxis, 0);
        }

        [Test]
        public void InterceptCalculatorFindsSolution()
        {
            var orbit = new OrbitData
            {
                orbit = new KeplerOrbit { semiMajorAxis = 5, gravitationalParameter = 1 }
            };
            var calc = new InterceptCalculator();
            var sol = calc.CalculateOrbitalIntercept(Vector3.zero, Vector3.zero, orbit, 1f, 1f);
            Assert.IsTrue(sol != null);
        }
    }
}
