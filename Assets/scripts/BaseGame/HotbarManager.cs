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
    public HotbarButton dogButton;  
    
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
    private bool isTutorial = false;
    
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        
        // Check if we're in tutorial mode
        isTutorial = TutorialLevelManager.Instance != null && TutorialLevelManager.Instance.isTutorial;
        
        SetupButtonMap();
        UpdateAllButtons();
    }
    
    void SetupButtonMap()
    {
        buttonMap[UnitType.MeleeOfficerV1] = meleeOfficerV1Button;
        buttonMap[UnitType.TankOfficer] = tankOfficerButton;
        buttonMap[UnitType.RangedOfficer] = rangedOfficerButton;
        buttonMap[UnitType.Dog] = dogButton;  
        
        meleeOfficerV1Button.Initialize(UnitType.MeleeOfficerV1, OnUnitSelected);
        tankOfficerButton.Initialize(UnitType.TankOfficer, OnUnitSelected);
        rangedOfficerButton.Initialize(UnitType.RangedOfficer, OnUnitSelected);
        dogButton.Initialize(UnitType.Dog, OnUnitSelected); 
        
        buffMap[BuffType.AttackBuff] = attackBuffButton;
        buffMap[BuffType.HealBuff] = healBuffButton;
        
        attackBuffButton.Initialize(BuffType.AttackBuff, OnBuffSelected, $"+{attackBuffDamage} Damage");
        healBuffButton.Initialize(BuffType.HealBuff, OnBuffSelected, $"+{healBuffAmount} Health");
    }
    
   
    
    public void ForceUpdateAllButtons()
    {
        UpdateAllButtons();
    }
    
    void UpdateAllButtons()
    {
        // Get the right game manager
        System.Collections.Generic.List<UnitType> unlockedUnits;
        int playerMoney;
        int currentWave;
        
        if (isTutorial)
        {
            if (TutGameManager.Instance == null) return;
            unlockedUnits = TutGameManager.Instance.unlockedUnits;
            playerMoney = TutGameManager.Instance.playerMoney;
            currentWave = TutGameManager.Instance.currentWave;
        }
        else
        {
            if (GameManager.Instance == null) return;
            unlockedUnits = GameManager.Instance.unlockedUnits;
            playerMoney = GameManager.Instance.playerMoney;
            currentWave = GameManager.Instance.currentWave;
        }
        
        // Update unit buttons
        foreach (var kvp in buttonMap)
        {
            UnitType unitType = kvp.Key;
            HotbarButton button = kvp.Value;
            
            bool isUnlocked = unlockedUnits.Contains(unitType);
            UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
            bool canAfford = playerMoney >= stats.cost;
            
            if (button != null)
            {
                button.UpdateVisuals(isUnlocked, canAfford, playerMoney);
            }
        }
        
      
        bool attackBuffUnlocked = currentWave >= 3;
        attackBuffButton.UpdateVisuals(attackBuffUnlocked);
        
        bool healBuffUnlocked = currentWave >= 4;
        healBuffButton.UpdateVisuals(healBuffUnlocked);
    }
    
    void OnUnitSelected(UnitType unitType)
    {
        // Get the right game manager
        System.Collections.Generic.List<UnitType> unlockedUnits;
        int playerMoney;
        
        if (isTutorial)
        {
            if (TutGameManager.Instance == null) return;
            unlockedUnits = TutGameManager.Instance.unlockedUnits;
            playerMoney = TutGameManager.Instance.playerMoney;
        }
        else
        {
            if (GameManager.Instance == null) return;
            unlockedUnits = GameManager.Instance.unlockedUnits;
            playerMoney = GameManager.Instance.playerMoney;
        }
        
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
        
        if (!unlockedUnits.Contains(unitType))
        {
            Debug.Log("Unit not unlocked yet!");
            return;
        }
        
        if (playerMoney < stats.cost)
        {
            Debug.Log($"Not enough money! Need R{stats.cost}");
            return;
        }
        
        inputManager.StartPlacingUnit(unitType);
    }
    
    void OnBuffSelected(BuffType buffType)
    {
        // Get the right game manager
        System.Collections.Generic.List<OfficerUnit> allUnits;
        
        if (isTutorial)
        {
            if (TutGameManager.Instance == null) return;
            allUnits = TutGameManager.Instance.GetActiveUnits();
        }
        else
        {
            if (GameManager.Instance == null) return;
            allUnits = GameManager.Instance.GetActiveUnits();
        }
        
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
    
    void ApplyAttackBuff(System.Collections.Generic.List<OfficerUnit> units)
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
    
    void ApplyHealBuff(System.Collections.Generic.List<OfficerUnit> units)
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
