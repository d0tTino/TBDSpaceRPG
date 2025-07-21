using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class SpaceshipMovementTests : TestClass {
    public SpaceshipMovementTests(Node scene) : base(scene) { }

    [Test]
    public void PhysicsProcess_ForwardActionAppliesThrust() {
        var movement = new SpaceshipMovement();
        Input.ActionPress("move_forward");
        movement.ThrustForce = 5f;
        movement.MaxSpeed = 100f;
        movement._PhysicsProcess(1.0);
        movement.Velocity.X.ShouldBe(0f);
        movement.Velocity.Y.ShouldBe(0f);
        movement.Velocity.Z.ShouldBe(-5f);
        Input.ActionRelease("move_forward");
    }

    [Test]
    public void PhysicsProcess_ClampsVelocity() {
        var movement = new SpaceshipMovement();
        Input.ActionPress("move_forward");
        movement.ThrustForce = 50f;
        movement.MaxSpeed = 10f;
        movement._PhysicsProcess(1.0);
        movement.Velocity.Length().ShouldBe(10f);
        Input.ActionRelease("move_forward");
    }
}
