using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyType
{
    MeleeV1,
    Tank,
    Ranged,
    MeleeV2WithDog
}

public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType;
    
    [Header("Enemy Stats")]
    public int health = 50;
    public float speed = 2f;
    public int damage = 10;
    public int moneyReward = 50;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    
    [Header("State")]
    public bool isAlive = true;
    private int currentHealth;
    private float lastAttackTime;
    
    [Header("Pathfinding")]
    private List<Vector2> path;
    private int waypointIndex = 0;
    
    [Header("Combat")]
    private OfficerUnit currentTarget;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    // For MeleeV2WithDog
    private GameObject dogCompanion;
    private SpriteRenderer dogSprite;
    private int dogHealth;
    private bool dogAlive = true;
    
    void Start()
    {
        currentHealth = health;
        
        // Get a random path from PathGenerator
        if (PathGenerator.Instance != null)
        {
           // path = new List<Vector2>(PathGenerator.Instance.GetRandomPath());
        }
        
        // Setup dog for MeleeV2
        if (enemyType == EnemyType.MeleeV2WithDog)
        {
            SpawnDogCompanion();
            dogHealth = 75;
        }
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // Check for nearby officers to attack
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNearbyOfficer();
        }
        
        if (currentTarget != null)
        {
            AttackOfficer();
        }
        else
        {
            MoveAlongPath();
        }
    }
    
    void MoveAlongPath()
    {
        if (path == null || path.Count == 0 || waypointIndex >= path.Count)
        {
            ReachTower();
            return;
        }
        
        Vector2 targetWaypoint = path[waypointIndex];
        Vector2 direction = (targetWaypoint - (Vector2)transform.position).normalized;
        
        // Move towards waypoint
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
        
        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        
        // Check if reached waypoint
        if (Vector2.Distance(transform.position, targetWaypoint) < 0.1f)
        {
            waypointIndex++;
        }
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }
    
    void FindNearbyOfficer()
    {
        OfficerUnit[] officers = FindObjectsOfType<OfficerUnit>();
        float closestDistance = attackRange;
        OfficerUnit closestOfficer = null;
        
        foreach (OfficerUnit officer in officers)
        {
            if (!officer.isAlive) continue;
            
            float distance = Vector2.Distance(transform.position, officer.transform.position);
            if (distance <= attackRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestOfficer = officer;
            }
        }
        
        currentTarget = closestOfficer;
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", currentTarget == null);
        }
    }
    
    void AttackOfficer()
    {
        // Face the target
        if (currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            
            if (spriteRenderer != null)
            {
                if (direction.x < 0)
                    spriteRenderer.flipX = true;
                else if (direction.x > 0)
                    spriteRenderer.flipX = false;
            }
        }
        
        // Attack on cooldown
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }
    
    void PerformAttack()
    {
        if (currentTarget != null && currentTarget.isAlive)
        {
            currentTarget.TakeDamage(damage);
            
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            StartCoroutine(AttackAnimation());
        }
        else
        {
            currentTarget = null;
        }
    }
    
    IEnumerator AttackAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.15f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
    
    public void TakeDamage(int damageAmount)
    {
        // If has dog and dog is alive, dog takes damage first
        if (enemyType == EnemyType.MeleeV2WithDog && dogAlive)
        {
            dogHealth -= damageAmount;
            
            if (dogSprite != null)
            {
                StartCoroutine(DamageFlashDog());
            }
            
            if (dogHealth <= 0)
            {
                KillDog();
                // Overflow damage goes to main unit
                int overflow = Mathf.Abs(dogHealth);
                if (overflow > 0)
                {
                    currentHealth -= overflow;
                }
            }
        }
        else
        {
            currentHealth -= damageAmount;
        }
        
        StartCoroutine(DamageFlash());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    IEnumerator DamageFlashDog()
    {
        if (dogSprite != null)
        {
            Color originalColor = dogSprite.color;
            dogSprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            dogSprite.color = originalColor;
        }
    }
    
    void SpawnDogCompanion()
    {
        dogAlive = true;
        
        dogCompanion = new GameObject("Dog");
        dogCompanion.transform.parent = transform;
        dogCompanion.transform.localPosition = new Vector3(0.6f, -0.3f, 0);
        dogCompanion.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        
        dogSprite = dogCompanion.AddComponent<SpriteRenderer>();
        dogSprite.color = new Color(0.6f, 0.4f, 0.2f); // Brown
        dogSprite.sortingLayerName = "Units";
        dogSprite.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 5;
        
        //  assign a dog sprite here if you have one
        // dogSprite.sprite = yourDogSprite;
    }
    
    void KillDog()
    {
        dogAlive = false;
        if (dogCompanion != null)
        {
            StartCoroutine(DogDeathAnimation());
        }
    }
    
    IEnumerator DogDeathAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 originalScale = dogCompanion.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (dogSprite != null)
            {
                Color c = dogSprite.color;
                c.a = 1f - t;
                dogSprite.color = c;
            }
            
            dogCompanion.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
        
        Destroy(dogCompanion);
    }
    
    void Die()
    {
        isAlive = false;
        
        // Give reward
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }
        
        // Notify spawner
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyKilled();
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        float duration = 0.4f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }
            
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void ReachTower()
    {
        // Deal damage to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damage);
        }
        
        // Notify spawner
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyReachedEnd();
        }
        
        Destroy(gameObject);
    }
}