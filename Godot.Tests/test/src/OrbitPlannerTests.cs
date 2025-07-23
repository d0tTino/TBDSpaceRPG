using System.Reflection;
using Godot;
using Chickensoft.GoDotTest;
using Shouldly;
using OrbitalMechanics;
using SimpleKeplerOrbits;

public class OrbitPlannerTests : TestClass {
    public OrbitPlannerTests(Node scene) : base(scene) { }

    [Test]
    public void PlanTransfer_ComputesAverageSemiMajorAxis() {
        var origin = new CelestialBody();
        var dest = new CelestialBody();

        typeof(CelestialBody).GetField("initialOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(origin, new OrbitData { orbit = new KeplerOrbit { semiMajorAxis = 10, gravitationalParameter = 1 } });
        typeof(CelestialBody).GetField("initialOrbit", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(dest, new OrbitData { orbit = new KeplerOrbit { semiMajorAxis = 20, gravitationalParameter = 1 } });

        var planner = new OrbitPlanner();
        var result = planner.PlanTransfer(origin, dest);

        result.ShouldNotBeNull();
        result.orbit.semiMajorAxis.ShouldBe(15);
    }
}
