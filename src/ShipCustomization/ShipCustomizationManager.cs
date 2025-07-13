using UnityEngine;

public class ShipCustomizationManager : MonoBehaviour
{
    public int thrustLevel = 1;
    public int speedLevel = 1;
    public int rotationLevel = 1;
    public int brakingLevel = 1;

    public SpaceshipMovement movement;

    private void Awake()
    {
        if (movement == null)
        {
            movement = GetComponent<SpaceshipMovement>();
        }
        ApplyUpgrades();
    }

    public void UpgradeThrust()
    {
        thrustLevel++;
        ApplyUpgrades();
    }

    public void UpgradeSpeed()
    {
        speedLevel++;
        ApplyUpgrades();
    }

    public void UpgradeRotation()
    {
        rotationLevel++;
        ApplyUpgrades();
    }

    public void UpgradeBraking()
    {
        brakingLevel++;
        ApplyUpgrades();
    }

    public void ResetUpgrades()
    {
        thrustLevel = speedLevel = rotationLevel = brakingLevel = 1;
        ApplyUpgrades();
    }

    public void LogCurrentUpgrades()
    {
        Debug.Log($"Thrust {thrustLevel}, Speed {speedLevel}, Rotation {rotationLevel}, Braking {brakingLevel}");
    }

    private void ApplyUpgrades()
    {
        if (movement == null) return;
        movement.thrustForce = 10f * thrustLevel;
        movement.maxSpeed = 20f * speedLevel;
        movement.rotationSpeed = 90f * rotationLevel;
        movement.brakingForce = 5f * brakingLevel;
    }
}
