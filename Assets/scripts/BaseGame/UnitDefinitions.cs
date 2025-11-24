using UnityEngine;

public enum UnitType
{
    MeleeOfficerV1,
    TankOfficer,
    RangedOfficer,
    Dog  // Replaced MeleeOfficerV2
}

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
}

public class UnitDefinitions : MonoBehaviour
{
    public static UnitDefinitions Instance;
    
    [Header("Unit Prefabs - Drag your officer prefabs here")]
    public GameObject meleeOfficerV1Prefab;
    public GameObject tankOfficerPrefab;
    public GameObject rangedOfficerPrefab;
    public GameObject dogPrefab;  // Replaced MeleeOfficerV2
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
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
                    attackRange = 8f,
                    damage = 15,
                    cost = 500,
                    attackCooldown = 1f
                };
                
            case UnitType.TankOfficer:
                return new UnitStats
                {
                    unitType = UnitType.TankOfficer,
                    unitName = "Tank Officer",
                    health = 150,
                    speed = 10f,
                    attackRange = 8f,
                    damage = 10,
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
                    attackRange = 16f,
                    damage = 20,
                    cost = 2000,
                    attackCooldown = 1.5f
                };
                
            case UnitType.Dog:
                return new UnitStats
                {
                    unitType = UnitType.Dog,
                    unitName = "Dog",
                    health = 75,
                    speed = 75f,
                    attackRange = 8f,
                    damage = 25,
                    cost = 800,
                    attackCooldown = 1.2f
                };
                
            default:
                Debug.LogError($"No stats defined for unit type: {type}");
                return null;
        }
    }
    
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
                
            case UnitType.Dog:
                return dogPrefab;
                
            default:
                Debug.LogError($"No prefab assigned for unit type: {type}");
                return null;
        }
    }
}