using Godot;

namespace OrbitalMechanics
{
    /// <summary>
    /// Follows a predefined orbit using data from OrbitData.
    /// </summary>
    
    public class OrbitFollower : Node3D
    {
        private OrbitData _orbit;

        public void SetOrbit(OrbitData orbit)
        {
            _orbit = orbit;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_orbit == null) return;
            double t = Time.GetTicksMsec()/1000.0;
            GlobalPosition = _orbit.GetPositionAtTime(t);
        }
    }
}
