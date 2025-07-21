using NUnit.Framework;
using Godot;

public class SpaceshipMovementTests
{
    [Test]
    public void PhysicsProcess_ForwardActionAppliesThrust()
    {
        var movement = new SpaceshipMovement();
        Input.ActionPress("move_forward");
        movement.ThrustForce = 5f;
        movement.MaxSpeed = 100f;
        movement._PhysicsProcess(1.0);
        Assert.AreEqual(0f, movement.Velocity.X);
        Assert.AreEqual(0f, movement.Velocity.Y);
        Assert.AreEqual(5f, movement.Velocity.Z);
        Input.ActionRelease("move_forward");
    }

    [Test]
    public void PhysicsProcess_ClampsVelocity()
    {
        var movement = new SpaceshipMovement();
        Input.ActionPress("move_forward");
        movement.ThrustForce = 50f;
        movement.MaxSpeed = 10f;
        movement._PhysicsProcess(1.0);
        Assert.AreEqual(10f, movement.Velocity.Length());
        Input.ActionRelease("move_forward");
    }
}
