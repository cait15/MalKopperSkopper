using UnityEngine;
using System.Collections.Generic;

public enum GamePhase
{
    Dialogue,
    Setup,
    Battle,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Phase")]
    public GamePhase currentPhase = GamePhase.Dialogue;
    
    [Header("Game State")]
    public int currentWave = 0;
    public int playerHealth = 90;
    public int playerMoney = 3589;
    public bool isGameOver = false;
    
    [Header("Wave Settings")]
    public int totalWaves = 10;
    public float setupPhaseDuration = 30f;
    public float victoryPhaseDuration = 5f;
    
    [Header("Unit Unlock Settings")]
    public List<UnitType> unlockedUnits = new List<UnitType>();
    
    private List<OfficerUnit> activeUnits = new List<OfficerUnit>();
    private float phaseTimer = 0f;
    private bool waveInProgress = false;
    private bool waitingForDialogue = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        unlockedUnits.Clear();
        currentWave = 0;
        isGameOver = false;
        currentPhase = GamePhase.Dialogue;
        
        // Unlock first unit immediately
        unlockedUnits.Add(UnitType.MeleeOfficerV1);
        
        Debug.Log("Game initialized. Starting Wave 1 Dialogue...");
        StartDialoguePhase(1);
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        // Don't process phases while waiting for dialogue
        if (waitingForDialogue) return;
        
        switch (currentPhase)
        {
            case GamePhase.Setup:
                UpdateSetupPhase();
                break;
                
            case GamePhase.Battle:
                UpdateBattlePhase();
                break;
                
            case GamePhase.Victory:
                UpdateVictoryPhase();
                break;
        }
        
        // Manual phase skip for testing
        if (Input.GetKeyDown(KeyCode.Return) && currentPhase == GamePhase.Setup)
        {
            Debug.Log("Skipping setup phase...");
            StartBattlePhase();
        }
    }
    
    // === DIALOGUE PHASE ===
    void StartDialoguePhase(int waveNumber)
    {
        currentPhase = GamePhase.Dialogue;
        waitingForDialogue = true;
        
        Debug.Log($"=== DIALOGUE PHASE - Wave {waveNumber} ===");
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogueForWave(waveNumber);
        }
        else
        {
            Debug.LogWarning("No DialogueManager found! Skipping to setup phase.");
            OnDialogueEnded();
        }
    }
    
    public void OnDialogueEnded()
    {
        waitingForDialogue = false;
        Debug.Log("Dialogue ended, moving to setup phase");
        StartSetupPhase();
    }
    
    // === SETUP PHASE ===
    void StartSetupPhase()
    {
        currentPhase = GamePhase.Setup;
        phaseTimer = setupPhaseDuration;
        
        Debug.Log("=== SETUP PHASE ===");
        Debug.Log($"Place your units! Time remaining: {setupPhaseDuration} seconds");
    }
    
    void UpdateSetupPhase()
    {
        phaseTimer -= Time.deltaTime;
        
        if (phaseTimer <= 0)
        {
            StartBattlePhase();
        }
    }
    
    // === BATTLE PHASE ===
    void StartBattlePhase()
    {
        currentPhase = GamePhase.Battle;
        phaseTimer = 0f;
        
        Debug.Log("=== BATTLE PHASE ===");
        Debug.Log("Defend your tower!");
        
        StartWave();
    }
    
    void UpdateBattlePhase()
    {
        if (isGameOver) return;
    }
    
    public void StartWave()
    {
        if (waveInProgress || isGameOver) return;
        
        currentWave++;
        waveInProgress = true;
        
        UnlockUnitsForWave(currentWave);
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StartWave(currentWave);
        }
        
        Debug.Log($"Wave {currentWave}/{totalWaves} started!");
    }
    
    void UnlockUnitsForWave(int wave)
    {
        switch (wave)
        {
            case 2:
                if (!unlockedUnits.Contains(UnitType.TankOfficer))
                {
                    unlockedUnits.Add(UnitType.TankOfficer);
                    Debug.Log("🔓 Tank Officer Unlocked!");
                }
                break;
            case 3:
                if (!unlockedUnits.Contains(UnitType.RangedOfficer))
                {
                    unlockedUnits.Add(UnitType.RangedOfficer);
                    Debug.Log("🔓 Ranged Officer Unlocked!");
                }
                break;
            case 6:
                if (!unlockedUnits.Contains(UnitType.MeleeOfficerV2))
                {
                    unlockedUnits.Add(UnitType.MeleeOfficerV2);
                    Debug.Log("🔓 Melee Officer V2 (with Dog) Unlocked!");
                }
                break;
        }
    }
    
    // === VICTORY PHASE ===
    public void OnWaveComplete()
    {
        if (isGameOver) return;  
        waveInProgress = false;
        
        Debug.Log($"Wave {currentWave} completed!");
        
        if (currentWave >= totalWaves)
        {
            GameWon();
        }
        else
        {
            StartVictoryPhase();
        }
    }
    
    void StartVictoryPhase()
    {
        currentPhase = GamePhase.Victory;
        phaseTimer = victoryPhaseDuration;
        
        Debug.Log("=== VICTORY ===");
        Debug.Log($"Wave {currentWave} cleared!");
    }
    
    void UpdateVictoryPhase()
    {
        phaseTimer -= Time.deltaTime;
        
        if (phaseTimer <= 0)
        {
            if (currentWave < totalWaves)
            {
                StartDialoguePhase(currentWave + 1);
            }
            else
            {
                GameWon();
            }
        }
    }
    
    // === GAME END ===
    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        Debug.Log($"Tower took {damage} damage! Health: {playerHealth}");
        
        if (playerHealth <= 0)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        isGameOver = true;
        currentPhase = GamePhase.Defeat; 
      
        
        Debug.Log("=== GAME OVER ===");
        Debug.Log("Your tower was destroyed!");
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDefeatDialogue();
        }
    }
    
    void GameWon()
    {
        Debug.Log("=== ULTIMATE VICTORY ===");
        Debug.Log($"All {totalWaves} waves completed! Tower health: {playerHealth}");
        currentPhase = GamePhase.Victory;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowVictoryDialogue();
        }
    }
    
    // === UTILITY ===
    public bool CanAfford(int cost)
    {
        return playerMoney >= cost;
    }
    
    public void SpendMoney(int amount)
    {
        playerMoney -= amount;
        Debug.Log($"Spent R{amount}. Money remaining: R{playerMoney}");
    }
    
    public void AddMoney(int amount)
    {
        playerMoney += amount;
    }
    
    public void RegisterUnit(OfficerUnit unit)
    {
        if (!activeUnits.Contains(unit))
        {
            activeUnits.Add(unit);
        }
    }
    
    public void UnregisterUnit(OfficerUnit unit)
    {
        if (activeUnits.Contains(unit))
        {
            activeUnits.Remove(unit);
        }
    }
    
    public List<OfficerUnit> GetActiveUnits()
    {
        return activeUnits;
    }
    
    // === GUI ===
    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.skin.box.fontSize = 16;
        
        // Phase indicator
        string phaseText = "";
        Color phaseColor = Color.white;
        
        switch (currentPhase)
        {
            case GamePhase.Dialogue:
                phaseText = "DIALOGUE PHASE";
                phaseColor = Color.cyan;
                break;
            case GamePhase.Setup:
                phaseText = $"SETUP PHASE - Time: {Mathf.CeilToInt(phaseTimer)}s";
                phaseColor = Color.green;
                break;
            case GamePhase.Battle:
                phaseText = "BATTLE PHASE";
                phaseColor = Color.red;
                break;
            case GamePhase.Victory:
                phaseText = "VICTORY!";
                phaseColor = Color.yellow;
                break;
            case GamePhase.Defeat:
                phaseText = "DEFEAT!";
                phaseColor = Color.yellow;
                break;
        }
        
        GUI.backgroundColor = phaseColor;
        GUI.Box(new Rect(Screen.width / 2 - 150, 10, 300, 40), phaseText);
        GUI.backgroundColor = Color.white;
        
        // Game stats
        GUI.skin.label.fontSize = 16;
        GUI.Label(new Rect(10, 60, 300, 30), $"Health: {playerHealth}");
        GUI.Label(new Rect(10, 90, 300, 30), $"Money: R{playerMoney}");
        GUI.Label(new Rect(10, 120, 300, 30), $"Wave: {currentWave}/{totalWaves}");
        
        // Setup phase hint
        if (currentPhase == GamePhase.Setup)
        {
            GUI.Label(new Rect(10, 170, 400, 30), "Press ENTER to skip setup");
        }
        
        // Game Over
        if (isGameOver)
        {
            GUI.skin.label.fontSize = 32;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), "GAME OVER");
        }
    }
}