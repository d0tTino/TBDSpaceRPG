using NUnit.Framework;
using UnityEngine;

public class SpaceshipMovementTests
{
    [Test]
    public void FixedUpdate_ForwardKeyAppliesThrust()
    {
        var rb = new Rigidbody();
        var movement = new SpaceshipMovement();
        typeof(SpaceshipMovement).GetField("_rb", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(movement, rb);
        Input.SetKey(KeyCode.W, true);
        movement.thrustForce = 5f;
        movement.maxSpeed = 100f;
        typeof(SpaceshipMovement).GetMethod("FixedUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(movement, null);
        Assert.AreEqual(0f, rb.velocity.x);
        Assert.AreEqual(0f, rb.velocity.y);
        Assert.AreEqual(5f, rb.velocity.z);
        Input.SetKey(KeyCode.W, false);
    }

    [Test]
    public void FixedUpdate_ClampsVelocity()
    {
        var rb = new Rigidbody();
        var movement = new SpaceshipMovement();
        typeof(SpaceshipMovement).GetField("_rb", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(movement, rb);
        Input.SetKey(KeyCode.W, true);
        movement.thrustForce = 50f;
        movement.maxSpeed = 10f;
        typeof(SpaceshipMovement).GetMethod("FixedUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(movement, null);
        Assert.AreEqual(10f, rb.velocity.magnitude);
        Input.SetKey(KeyCode.W, false);
    }
}
