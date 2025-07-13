#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ShipUpgradeMenuHelper
{
    [MenuItem("MCP/Ship/Create Test Ship")]
    public static void CreateTestShip()
    {
        var ship = new GameObject("TestShip");
        ship.AddComponent<Rigidbody>();
        ship.AddComponent<SpaceshipMovement>();
        ship.AddComponent<ShipCustomizationManager>();
        Selection.activeGameObject = ship;
    }

    [MenuItem("MCP/Ship/Upgrade Thrust")]
    public static void UpgradeThrust() => GetManager()?.UpgradeThrust();

    [MenuItem("MCP/Ship/Upgrade Speed")]
    public static void UpgradeSpeed() => GetManager()?.UpgradeSpeed();

    [MenuItem("MCP/Ship/Upgrade Rotation")]
    public static void UpgradeRotation() => GetManager()?.UpgradeRotation();

    [MenuItem("MCP/Ship/Upgrade Braking")]
    public static void UpgradeBraking() => GetManager()?.UpgradeBraking();

    [MenuItem("MCP/Ship/Reset All Upgrades")]
    public static void ResetUpgrades() => GetManager()?.ResetUpgrades();

    [MenuItem("MCP/Ship/Log Current Upgrades")]
    public static void LogUpgrades() => GetManager()?.LogCurrentUpgrades();

    private static ShipCustomizationManager GetManager()
    {
        var obj = Selection.activeGameObject;
        return obj ? obj.GetComponent<ShipCustomizationManager>() : null;
    }
}
#endif
