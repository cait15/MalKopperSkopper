using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    
    [Header("Enemy Prefabs")]
    public GameObject meleeV1Prefab;
    public GameObject tankPrefab;
    public GameObject rangedPrefab;
    public GameObject meleeV2Prefab;
    public GameObject bossPrefab;
    
    [Header("Spawn Height")]
    public float spawnHeight = 0f;
    
    [Header("Wave Tracking")]
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private int enemiesReachedEnd = 0;
    private bool isSpawning = false;
    private bool gameOver = false;
    
    private WaveConfiguration currentWave;
    private List<GameObject> spawnerObjects;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        spawnerObjects = new List<GameObject>(spawners);
        
        if (spawnerObjects.Count == 0)
        {
            Debug.LogError("No spawner objects found with 'Spawner' tag!");
        }
        else
        {
            Debug.Log($"Found {spawnerObjects.Count} spawner(s)");
        }
    }
    
    public void StartWave(int waveNumber)
    {
        if (isSpawning || gameOver) return;
    
        // Check if tutorial is active
        if (TutorialLevelManager.Instance != null && TutorialLevelManager.Instance.isTutorial)
        {
            StartTutorialWave();
            return;
        }
    
        currentWave = GetWaveConfiguration(waveNumber);
        enemiesSpawned = 0;
        enemiesAlive = 0;
        enemiesReachedEnd = 0;
    
        Debug.Log($"Starting {currentWave.description}");
    
        StartCoroutine(SpawnWave());
    }
    
    public void StartTutorialWave()
    {
        if (isSpawning || gameOver) return;
    
        currentWave = GetTutorialWaveConfiguration();
        enemiesSpawned = 0;
        enemiesAlive = 0;
        enemiesReachedEnd = 0;
    
        Debug.Log($"Starting {currentWave.description}");
    
        StartCoroutine(SpawnWave());
    }

    WaveConfiguration GetTutorialWaveConfiguration()
    {
        WaveConfiguration config = new WaveConfiguration();
        config.waveNumber = 1;
        config.description = "Tutorial Wave: Easy Introduction";
        config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1 };
        config.enemyCount = 5;
        config.spawnInterval = 3.0f;
        config.hasBoss = false;
    
        return config;
    }
    
    WaveConfiguration GetWaveConfiguration(int waveNumber)
    {
        WaveConfiguration config = new WaveConfiguration();
        config.waveNumber = waveNumber;
        config.hasBoss = (waveNumber == 5 || waveNumber == 10);
        
        switch (waveNumber)
        {
            case 1:
                config.description = "Wave 1: Introduction - Melee Enemies V1";
                config.enemyTypes = new List<EnemyType> { EnemyType.Ranged };
                config.enemyCount = 10;
                config.spawnInterval = 2.0f;
                break;
                
            case 2:
                config.description = "Wave 2: Tank Enemies Join";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1, EnemyType.Tank };
                config.enemyCount = 12;
                config.spawnInterval = 2.0f;
                break;
                
            case 3:
                config.description = "Wave 3: Ranged Enemies Appear";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged };
                config.enemyCount = 15;
                config.spawnInterval = 2.0f;
                break;
                
            case 4:
                config.description = "Wave 4: Intensity Increases";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged };
                config.enemyCount = 18;
                config.spawnInterval = 1.5f;
                break;
                
            case 5:
                config.description = "Wave 5: BOSS APPEARS - All Enemy Types";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged };
                config.enemyCount = 15;
                config.spawnInterval = 1.5f;
                break;
                
            case 6:
                config.description = "Wave 6: Dogs Unleashed - All Enemy Types";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, 
                };
                config.enemyCount = 22;
                config.spawnInterval = 1.3f;
                break;
                
            case 7:
                config.description = "Wave 7: Heavy Assault";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, 
                };
                config.enemyCount = 25;
                config.spawnInterval = 1.0f;
                break;
                
            case 8:
                config.description = "Wave 8: Overwhelming Force";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, 
                };
                config.enemyCount = 28;
                config.spawnInterval = 0.9f;
                break;
                
            case 9:
                config.description = "Wave 9: Near Breaking Point";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, 
                };
                config.enemyCount = 32;
                config.spawnInterval = 0.8f;
                break;
                
            case 10:
                config.description = "Wave 10: FINAL BOSS - Ultimate Challenge";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged,
                };
                config.enemyCount = 30;
                config.spawnInterval = 0.7f;
                break;
                
            default:
                config.description = $"Wave {waveNumber}: Endless Mode";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged
                };
                config.enemyCount = 40 + ((waveNumber - 10) * 5);
                config.spawnInterval = Mathf.Max(0.5f, 0.7f - ((waveNumber - 10) * 0.05f));
                break;
        }
        
        return config;
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (gameOver)
            {
                isSpawning = false;
                yield break;
            }
            
            SpawnEnemy();
            enemiesSpawned++;
            
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }
        
        // Spawn boss at the end if this is a boss wave
        if (currentWave.hasBoss)
        {
            Debug.Log("Boss wave detected, spawning boss in 2 seconds...");
            yield return new WaitForSeconds(2f);
            SpawnBoss();
            Debug.Log("Boss spawn complete!");
        }
        
        isSpawning = false;
        Debug.Log($"Wave spawn complete. isSpawning set to false. EnemiesAlive: {enemiesAlive}");
    }
    
    void SpawnEnemy()
    {
        if (spawnerObjects.Count == 0)
        {
            Debug.LogError("No spawner objects available!");
            return;
        }

        GameObject spawner = spawnerObjects[Random.Range(0, spawnerObjects.Count)];
        Vector3 spawnPos = spawner.transform.position;
        spawnPos.y = spawnHeight;

        EnemyType enemyType = currentWave.enemyTypes[
            Random.Range(0, currentWave.enemyTypes.Count)
        ];

        GameObject enemyPrefab = GetEnemyPrefab(enemyType);

        if (enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not found for type: {enemyType}");
            return;
        }

        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos + randomOffset, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.enemyType = enemyType;
            ConfigureEnemyStats(enemy, enemyType, currentWave.waveNumber);
            enemiesAlive++;
        }
    }
    
    void SpawnBoss()
    {
        Debug.Log($"SpawnBoss called! bossPrefab null: {bossPrefab == null}, spawnerCount: {spawnerObjects.Count}");
        
        if (spawnerObjects.Count == 0)
        {
            Debug.LogError("No spawner objects available for boss!");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab not assigned!");
            return;
        }

        GameObject spawner = spawnerObjects[Random.Range(0, spawnerObjects.Count)];
        Vector3 spawnPos = spawner.transform.position;
        spawnPos.y = spawnHeight;

        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Enemy boss = bossObj.GetComponent<Enemy>();

        if (boss != null)
        {
            boss.enemyType = EnemyType.Boss;
            ConfigureBossStats(boss, currentWave.waveNumber);
            enemiesAlive++;
            Debug.Log($"<color=red>BOSS SPAWNED! Wave {currentWave.waveNumber}</color>");
        }
        else
        {
            Debug.LogError("Boss prefab doesn't have Enemy component!");
        }
    }

    GameObject GetEnemyPrefab(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.MeleeV1:
                return meleeV1Prefab;
            case EnemyType.Tank:
                return tankPrefab;
            case EnemyType.Ranged:
                return rangedPrefab;
           
            case EnemyType.Boss:
                return bossPrefab;
            default:
                return null;
        }
    }
    
    void ConfigureEnemyStats(Enemy enemy, EnemyType type, int waveNumber)
    {
        switch (type)
        {
            case EnemyType.MeleeV1:
                enemy.health = 50 + (waveNumber * 8);
                enemy.speed = 5f;
                enemy.damage = 3 + (waveNumber * 2);
                enemy.moneyReward = 50;
                enemy.attackRange = 6f;
                enemy.attackCooldown = 1.2f;
                break;
                
            case EnemyType.Tank:
                enemy.health = 120 + (waveNumber * 15);
                enemy.speed = 2f;
                enemy.damage = 20 + (waveNumber * 3);
                enemy.moneyReward = 100;
                enemy.attackRange = 4.5f;
                enemy.attackCooldown = 2f;
                break;
                
            case EnemyType.Ranged:
                enemy.health = 60 + (waveNumber * 6);
                enemy.speed = 4.5f;
                enemy.damage = 15 + (waveNumber * 2);
                enemy.moneyReward = 75;
                enemy.attackRange = 6f;
                enemy.attackCooldown = 1.5f;
                break;
                
          
        }
    }
    
    void ConfigureBossStats(Enemy boss, int waveNumber)
    {
        // Boss has massive HP
        boss.health = 500 + (waveNumber * 100);
        boss.speed = 1.5f;
        boss.damage = 35 + (waveNumber * 5);
        boss.moneyReward = 500;
        boss.attackRange = 4f;
        boss.attackCooldown = 1.5f;
    }
    
    public void OnEnemyKilled()
    {
        if (gameOver) return;
        
        enemiesAlive--;
        CheckWaveComplete();
    }
    
    public void OnEnemyReachedEnd()
    {
        if (gameOver) return;
        
        enemiesAlive--;
        enemiesReachedEnd++;
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
    {
        if (!isSpawning && enemiesAlive <= 0)
        {
            Debug.Log($"Wave {currentWave.waveNumber} Complete! Enemies reached tower: {enemiesReachedEnd}");
            
            // Works with both GameManager and TutGameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveComplete();
            }
            else if (TutGameManager.Instance != null)
            {
                TutGameManager.Instance.OnWaveComplete();
            }
        }
    }
    
    public void OnGameOver()
    {
        gameOver = true;
        isSpawning = false;
        
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        
        Debug.Log("Game Over - All enemies deactivated");
    }
}

[System.Serializable]
public class WaveConfiguration
{
    public int waveNumber;
    public List<EnemyType> enemyTypes;
    public int enemyCount;
    public float spawnInterval;
    public string description;
    public bool hasBoss;
}