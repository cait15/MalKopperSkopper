using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public enum BuffType
{
    AttackBuff,
    HealBuff
}

public class HotbarUI : MonoBehaviour
{
    [Header("Unit Buttons")]
    public HotbarButton meleeOfficerV1Button;
    public HotbarButton tankOfficerButton;
    public HotbarButton rangedOfficerButton;
    public HotbarButton meleeOfficerV2Button;
    
    [Header("Buff Buttons")]
    public HotbarBuffButton attackBuffButton;
    public HotbarBuffButton healBuffButton;
    
    [Header("Buff Settings")]
    public int attackBuffDamage = 15;
    public float attackBuffDuration = 5f;
    public int healBuffAmount = 50;
    
    private Dictionary<UnitType, HotbarButton> buttonMap = new Dictionary<UnitType, HotbarButton>();
    private Dictionary<BuffType, HotbarBuffButton> buffMap = new Dictionary<BuffType, HotbarBuffButton>();
    private InputManager inputManager;
    private GameManager gameManager;
    
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        gameManager = GameManager.Instance;
        SetupButtonMap();
        UpdateAllButtons();
    }
    
    void SetupButtonMap()
    {
        buttonMap[UnitType.MeleeOfficerV1] = meleeOfficerV1Button;
        buttonMap[UnitType.TankOfficer] = tankOfficerButton;
        buttonMap[UnitType.RangedOfficer] = rangedOfficerButton;
        buttonMap[UnitType.MeleeOfficerV2] = meleeOfficerV2Button;
        
        meleeOfficerV1Button.Initialize(UnitType.MeleeOfficerV1, OnUnitSelected);
        tankOfficerButton.Initialize(UnitType.TankOfficer, OnUnitSelected);
        rangedOfficerButton.Initialize(UnitType.RangedOfficer, OnUnitSelected);
        meleeOfficerV2Button.Initialize(UnitType.MeleeOfficerV2, OnUnitSelected);
        
        buffMap[BuffType.AttackBuff] = attackBuffButton;
        buffMap[BuffType.HealBuff] = healBuffButton;
        
        attackBuffButton.Initialize(BuffType.AttackBuff, OnBuffSelected, $"+{attackBuffDamage} Damage");
        healBuffButton.Initialize(BuffType.HealBuff, OnBuffSelected, $"+{healBuffAmount} Health");
    }
    
    void Update()
    {
    }
    
    public void ForceUpdateAllButtons()
    {
        UpdateAllButtons();
    }
    
    void UpdateAllButtons()
    {
        foreach (var kvp in buttonMap)
        {
            UnitType unitType = kvp.Key;
            HotbarButton button = kvp.Value;
            
            bool isUnlocked = gameManager.unlockedUnits.Contains(unitType);
            UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
            bool canAfford = gameManager.CanAfford(stats.cost);
            
            if (button != null)
            {
                button.UpdateVisuals(isUnlocked, canAfford, gameManager.playerMoney);
            }
        }
        
        bool attackBuffUnlocked = gameManager.currentWave >= 3;
        attackBuffButton.UpdateVisuals(attackBuffUnlocked);
        
        bool healBuffUnlocked = gameManager.currentWave >= 4;
        healBuffButton.UpdateVisuals(healBuffUnlocked);
    }
    
    void OnUnitSelected(UnitType unitType)
    {
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
        
        if (!gameManager.unlockedUnits.Contains(unitType))
        {
            Debug.Log("Unit not unlocked yet!");
            return;
        }
        
        if (!gameManager.CanAfford(stats.cost))
        {
            Debug.Log($"Not enough money! Need R{stats.cost}");
            return;
        }
        
        inputManager.StartPlacingUnit(unitType);
    }
    
    void OnBuffSelected(BuffType buffType)
    {
        List<OfficerUnit> allUnits = gameManager.GetActiveUnits();
        
        if (allUnits.Count == 0)
        {
            Debug.Log("No units placed to buff!");
            return;
        }
        
        switch (buffType)
        {
            case BuffType.AttackBuff:
                ApplyAttackBuff(allUnits);
                break;
            case BuffType.HealBuff:
                ApplyHealBuff(allUnits);
                break;
        }
    }
    
    void ApplyAttackBuff(List<OfficerUnit> units)
    {
        foreach (OfficerUnit unit in units)
        {
            if (unit != null && unit.isAlive)
            {
                unit.ApplyTemporaryDamageBuff(attackBuffDamage, attackBuffDuration);
                Debug.Log($"{unit.stats.unitName} gained +{attackBuffDamage} damage for {attackBuffDuration}s!");
            }
        }
    }
    
    void ApplyHealBuff(List<OfficerUnit> units)
    {
        foreach (OfficerUnit unit in units)
        {
            if (unit != null && unit.isAlive)
            {
                unit.Heal(healBuffAmount);
                Debug.Log($"{unit.stats.unitName} healed for {healBuffAmount}!");
            }
        }
    }
}