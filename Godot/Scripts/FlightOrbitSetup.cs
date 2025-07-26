using Godot;
using OrbitalMechanics;

public partial class FlightOrbitSetup : Node
{
    [Export] public NodePath ShipPath = "Ship";
    [Export] public NodePath BodyPath = "Planet";
    [Export] public double OrbitRadius = 20.0;

    public override void _Ready()
    {
        var ship = GetNode<Node3D>(ShipPath);
        var body = GetNode<CelestialBody>(BodyPath);
        if (ship == null || body == null) return;

        var follower = new OrbitFollower();
        AddChild(follower);
        follower.SetOrbit(new OrbitPlanner().PlanCircularOrbit(body, OrbitRadius));
        ship.GetParent()?.RemoveChild(ship);
        follower.AddChild(ship);
    }
}
