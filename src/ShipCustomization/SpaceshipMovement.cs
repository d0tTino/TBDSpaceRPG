using Godot;

public partial class SpaceshipMovement : CharacterBody3D
{
    [Export] public float ThrustForce = 10f;
    [Export] public float MaxSpeed = 20f;
    [Export] public float RotationSpeed = 90f;
    [Export] public float BrakingForce = 5f;

    public override void _PhysicsProcess(double delta)
    {
        var vel = Velocity;
        float dt = (float)delta;

        if (Input.IsActionPressed("move_forward"))
            vel += -Transform.Basis.Z * ThrustForce * dt;
        if (Input.IsActionPressed("move_backward"))
            vel += Transform.Basis.Z * BrakingForce * dt;
        if (Input.IsActionPressed("turn_left"))
            RotateY(Mathf.DegToRad(-RotationSpeed * dt));
        if (Input.IsActionPressed("turn_right"))
            RotateY(Mathf.DegToRad(RotationSpeed * dt));

        vel = vel.LimitLength(MaxSpeed);
        Velocity = vel;
        MoveAndSlide();
    }
}
