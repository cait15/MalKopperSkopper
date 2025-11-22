using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

    [Header("Game Phase")] public GamePhase currentPhase = GamePhase.Dialogue;

    [Header("Game State")] public int currentWave = 0;
    public int playerHealth = 90;
    public int playerMoney = 3589;
    public bool isGameOver = false;

    [Header("Wave Settings")] public int totalWaves = 10;
    public float setupPhaseDuration = 30f;
    public float victoryPhaseDuration = 5f;
    
    [Header("Money Rewards")]
    public int moneyPerWaveComplete = 200;
    public int moneyPerEnemyKill = 25;

    [Header("Unit Unlock Settings")] public List<UnitType> unlockedUnits = new List<UnitType>();

    [Header("UI References (TMP)")] 
    public TMP_Text healthText;
    public TMP_Text moneyText;
    public TMP_Text waveText;
    public TMP_Text phaseText;
    public TMP_Text gameOverText;
    public GameObject gameoverText;
    public HotbarUI hotbarUI;

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
        gameoverText.SetActive(false);
    }

    void Update()
    {
        // Update UI every frame so timer displays correctly
        UpdateUI();
        
        // Force update hotbar buttons even when hidden
        if (hotbarUI != null)
        {
            hotbarUI.ForceUpdateAllButtons();
        }
        
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

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {playerHealth}";

        if (moneyText != null)
            moneyText.text = $"Money: R{playerMoney}";

        if (waveText != null)
            waveText.text = $"Wave: {currentWave}/{totalWaves}";

        if (phaseText != null)
            phaseText.text = GetPhaseText();
    }

    string GetPhaseText()
    {
        switch (currentPhase)
        {
            case GamePhase.Dialogue: return "DIALOGUE PHASE";
            case GamePhase.Setup: return $"SETUP PHASE ({Mathf.CeilToInt(phaseTimer)}s)";
            case GamePhase.Battle: return "BATTLE PHASE";
            case GamePhase.Victory: return "VICTORY!";
            case GamePhase.Defeat: return "DEFEAT!";
            default: return "";
        }
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

    // === DIALOGUE PHASE ===
    void StartDialoguePhase(int waveNumber)
    {
        currentPhase = GamePhase.Dialogue;
        waitingForDialogue = true;
        UpdateUI();
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
        
        // Unlock units for next wave BEFORE setup phase starts
        UnlockUnitsForWave(currentWave + 1);
        
        StartSetupPhase();
    }

    // === SETUP PHASE ===
    void StartSetupPhase()
    {
        UpdateUI();
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
        UpdateUI();

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
        UpdateUI();

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StartWave(currentWave);
        }

        Debug.Log($"Wave {currentWave}/{totalWaves} started!");
    }

    void UnlockUnitsForWave(int wave)
    {
        Debug.Log($"[UNLOCK CHECK] Wave {wave} - Checking unit unlocks...");
        
        switch (wave)
        {
            case 2:
                if (!unlockedUnits.Contains(UnitType.TankOfficer))
                {
                    unlockedUnits.Add(UnitType.TankOfficer);
                    Debug.Log("<color=yellow>[UNLOCK] Tank Officer Unlocked!</color>");
                }
                else
                {
                    Debug.Log("[UNLOCK] Tank Officer already unlocked");
                }
                break;
                
            case 3:
                if (!unlockedUnits.Contains(UnitType.RangedOfficer))
                {
                    unlockedUnits.Add(UnitType.RangedOfficer);
                    Debug.Log("<color=yellow>[UNLOCK] Ranged Officer Unlocked!</color>");
                }
                else
                {
                    Debug.Log("[UNLOCK] Ranged Officer already unlocked");
                }
                break;
                
            case 6:
                if (!unlockedUnits.Contains(UnitType.MeleeOfficerV2))
                {
                    unlockedUnits.Add(UnitType.MeleeOfficerV2);
                    Debug.Log("<color=yellow>[UNLOCK] Melee Officer V2 (with Dog) Unlocked!</color>");
                }
                else
                {
                    Debug.Log("[UNLOCK] Melee Officer V2 already unlocked");
                }
                break;
                
            default:
                Debug.Log($"[UNLOCK CHECK] Wave {wave} has no unit unlocks");
                break;
        }
    }

    // === VICTORY PHASE ===
    public void OnWaveComplete()
    {
        if (isGameOver) return;
        waveInProgress = false;

        // Give money reward for completing wave
        AddMoney(moneyPerWaveComplete);

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
        UpdateUI();
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
        gameoverText.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER";
        }

        Debug.Log("=== GAME OVER ===");
        Debug.Log("Your tower was destroyed!");
        
        // Stop enemies and timer
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnGameOver();
        }

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
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "VICTORY!";
        }

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
        UpdateUI();
        Debug.Log($"Spent R{amount}. Money remaining: R{playerMoney}");
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        UpdateUI();
        Debug.Log($"Gained R{amount}. Money: R{playerMoney}");
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
}