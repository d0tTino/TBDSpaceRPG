using NUnit.Framework;
using UnityEngine;
using OrbitalMechanics.Combat;
using OrbitalMechanics;

namespace Tests.EditMode
{
    class DummyOrbitData : OrbitData
    {
        new public Vector3 GetPositionAtTime(double time)
        {
            return new Vector3((float)time, 0f, 0f);
        }
    }

    public class OrbitalMechanicsTests
    {
        [Test]
        public void InterceptCalculator_ReturnsSolution()
        {
            var calc = new InterceptCalculator();
            var orbit = new DummyOrbitData();
            var solution = calc.CalculateOrbitalIntercept(Vector3.zero, Vector3.zero, orbit, 1f, 5f);
            Assert.IsTrue(solution.HasValue);
        }
    }
}
