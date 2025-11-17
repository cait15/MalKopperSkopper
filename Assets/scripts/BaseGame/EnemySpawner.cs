using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveConfiguration
{
    public int waveNumber;
    public List<EnemyType> enemyTypes;
    public int enemyCount;
    public float spawnInterval;
    public string description;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    
    [Header("Enemy Prefabs")]
    public GameObject meleeV1Prefab;
    public GameObject tankPrefab;
    public GameObject rangedPrefab;
    public GameObject meleeV2Prefab;
    
    [Header("Wave Tracking")]
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private int enemiesReachedEnd = 0;
    private bool isSpawning = false;
    
    private WaveConfiguration currentWave;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void StartWave(int waveNumber)
    {
        if (isSpawning) return;
        
        currentWave = GetWaveConfiguration(waveNumber);
        enemiesSpawned = 0;
        enemiesAlive = 0;
        enemiesReachedEnd = 0;
        
        Debug.Log($"Starting {currentWave.description}");
        
        StartCoroutine(SpawnWave());
    }
    
    WaveConfiguration GetWaveConfiguration(int waveNumber)
    {
        WaveConfiguration config = new WaveConfiguration();
        config.waveNumber = waveNumber;
        
        switch (waveNumber)
        {
            case 1:
                config.description = "Wave 1: Introduction - Melee Enemies V1";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1 };
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
                config.description = "Wave 5: Continued Pressure";
                config.enemyTypes = new List<EnemyType> { EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged };
                config.enemyCount = 20;
                config.spawnInterval = 1.5f;
                break;
                
            case 6:
                config.description = "Wave 6: Dogs Unleashed - All Enemy Types";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
                };
                config.enemyCount = 22;
                config.spawnInterval = 1.3f;
                break;
                
            case 7:
                config.description = "Wave 7: Heavy Assault";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
                };
                config.enemyCount = 25;
                config.spawnInterval = 1.0f;
                break;
                
            case 8:
                config.description = "Wave 8: Overwhelming Force";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
                };
                config.enemyCount = 28;
                config.spawnInterval = 0.9f;
                break;
                
            case 9:
                config.description = "Wave 9: Near Breaking Point";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
                };
                config.enemyCount = 32;
                config.spawnInterval = 0.8f;
                break;
                
            case 10:
                config.description = "Wave 10: FINAL WAVE - Ultimate Challenge";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
                };
                config.enemyCount = 40;
                config.spawnInterval = 0.7f;
                break;
                
            default:
                config.description = $"Wave {waveNumber}: Endless Mode";
                config.enemyTypes = new List<EnemyType> { 
                    EnemyType.MeleeV1, EnemyType.Tank, EnemyType.Ranged, EnemyType.MeleeV2WithDog 
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
            SpawnEnemy();
            enemiesSpawned++;
            
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }
        
        isSpawning = false;
    }
    
    void SpawnEnemy()
    {
        // Choose random enemy type from available types for this wave
        EnemyType enemyType = currentWave.enemyTypes[Random.Range(0, currentWave.enemyTypes.Count)];
        
        // Get appropriate prefab
        GameObject enemyPrefab = GetEnemyPrefab(enemyType);
        
        if (enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not found for type: {enemyType}");
            return;
        }
        
        // Get spawn point from PathGenerator
        Vector2 spawnPos = Vector2.zero;
        if (PathGenerator.Instance != null)
        {
            spawnPos = PathGenerator.Instance.GetRandomSpawnPoint();
        }
        
        // Spawn enemy
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.enemyType = enemyType;
            ConfigureEnemyStats(enemy, enemyType, currentWave.waveNumber);
            enemiesAlive++;
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
            case EnemyType.MeleeV2WithDog:
                return meleeV2Prefab;
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
                enemy.speed = 3.5f;
                enemy.damage = 10 + (waveNumber * 2);
                enemy.moneyReward = 50;
                enemy.attackRange = 2f;
                enemy.attackCooldown = 1.2f;
                break;
                
            case EnemyType.Tank:
                enemy.health = 120 + (waveNumber * 15);
                enemy.speed = 2f;
                enemy.damage = 20 + (waveNumber * 3);
                enemy.moneyReward = 100;
                enemy.attackRange = 2f;
                enemy.attackCooldown = 2f;
                break;
                
            case EnemyType.Ranged:
                enemy.health = 60 + (waveNumber * 6);
                enemy.speed = 2.5f;
                enemy.damage = 15 + (waveNumber * 2);
                enemy.moneyReward = 75;
                enemy.attackRange = 6f;
                enemy.attackCooldown = 1.5f;
                break;
                
            case EnemyType.MeleeV2WithDog:
                enemy.health = 80 + (waveNumber * 10);
                enemy.speed = 3f;
                enemy.damage = 12 + (waveNumber * 2);
                enemy.moneyReward = 120;
                enemy.attackRange = 2.5f;
                enemy.attackCooldown = 1f;
                break;
        }
    }
    
    public void OnEnemyKilled()
    {
        enemiesAlive--;
        CheckWaveComplete();
    }
    
    public void OnEnemyReachedEnd()
    {
        enemiesAlive--;
        enemiesReachedEnd++;
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
    {
        if (!isSpawning && enemiesAlive <= 0)
        {
            Debug.Log($"Wave {currentWave.waveNumber} Complete! Enemies reached tower: {enemiesReachedEnd}");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveComplete();
            }
        }
    }
}