using UnityEngine;

// Enum for different unit types
public enum UnitType
{
    MeleeOfficerV1,
    TankOfficer,
    RangedOfficer,
    MeleeOfficerV2
}

// Data class to hold unit statistics
[System.Serializable]
public class UnitStats
{
    public UnitType unitType;
    public string unitName;
    public int health;
    public float speed;
    public float attackRange;
    public int damage;
    public int cost;
    public float attackCooldown;
    
    // Special stats for Melee Officer V2 (with dog)
    public int dogHealth;
    public float dogSpeed;
    public int dogDamage;
    public float dogRange;
}

public class UnitDefinitions : MonoBehaviour
{
    public static UnitDefinitions Instance;
    
    [Header("Unit Prefabs - Drag your officer prefabs here")]
    public GameObject meleeOfficerV1Prefab;
    public GameObject tankOfficerPrefab;
    public GameObject rangedOfficerPrefab;
    public GameObject meleeOfficerV2Prefab;
    
    void Awake()
    {
        // Singleton pattern - only one instance exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // Get the stats for a specific unit type
    public UnitStats GetUnitStats(UnitType type)
    {
        switch (type)
        {
            case UnitType.MeleeOfficerV1:
                return new UnitStats
                {
                    unitType = UnitType.MeleeOfficerV1,
                    unitName = "Melee Officer V1",
                    health = 100,
                    speed = 75f,
                    attackRange = 5f,
                    damage = 10,
                    cost = 500,
                    attackCooldown = 1f
                };
                
            case UnitType.TankOfficer:
                return new UnitStats
                {
                    unitType = UnitType.TankOfficer,
                    unitName = "Tank Officer",
                    health = 150,
                    speed = 25f,
                    attackRange = 3f,
                    damage = 25,
                    cost = 1000,
                    attackCooldown = 2f
                };
                
            case UnitType.RangedOfficer:
                return new UnitStats
                {
                    unitType = UnitType.RangedOfficer,
                    unitName = "Ranged Officer",
                    health = 100,
                    speed = 50f,
                    attackRange = 6f,
                    damage = 15,
                    cost = 2000,
                    attackCooldown = 1.5f
                };
                
            case UnitType.MeleeOfficerV2:
                return new UnitStats
                {
                    unitType = UnitType.MeleeOfficerV2,
                    unitName = "Melee Officer V2 (with Dog)",
                    health = 100,
                    speed = 50f,
                    attackRange = 5f,
                    damage = 10,
                    cost = 1500,
                    attackCooldown = 1f,
                    // Dog stats
                    dogHealth = 75,
                    dogSpeed = 75f,
                    dogDamage = 15,
                    dogRange = 2f
                };
                
            default:
                Debug.LogError($"No stats defined for unit type: {type}");
                return null;
        }
    }
    
    // Get the prefab for a specific unit type
    public GameObject GetUnitPrefab(UnitType type)
    {
        switch (type)
        {
            case UnitType.MeleeOfficerV1:
                return meleeOfficerV1Prefab;
                
            case UnitType.TankOfficer:
                return tankOfficerPrefab;
                
            case UnitType.RangedOfficer:
                return rangedOfficerPrefab;
                
            case UnitType.MeleeOfficerV2:
                return meleeOfficerV2Prefab;
                
            default:
                Debug.LogError($"No prefab assigned for unit type: {type}");
                return null;
        }
    }
}