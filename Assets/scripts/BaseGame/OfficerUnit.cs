using UnityEngine;
using System.Collections;

public class OfficerUnit : MonoBehaviour
{
    [Header("Unit Stats")]
    public UnitStats stats;
    public int currentHealth;
    public bool isAlive = true;
    
    [Header("Combat")]
    private Enemy currentTarget;
    private float lastAttackTime;
    
    [Header("Buffs")]
    public int temporaryHealthBonus = 0;
    public int temporaryDamageBonus = 0;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    [Header("Visual Feedback")]
    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    
    // For Melee Officer V2
    private GameObject dogCompanion;
    private int dogCurrentHealth;
    private bool hasDog = false;
    
    void Start()
    {
        if (stats == null)
        {
            Debug.LogError("OfficerUnit has no stats assigned!");
            return;
        }
        
        currentHealth = stats.health;
        GameManager.Instance.RegisterUnit(this);
        
        // Create dog companion for MeleeOfficerV2
        if (stats.unitType == UnitType.MeleeOfficerV2)
        {
            SpawnDogCompanion();
        }
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // Find and attack enemies
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNewTarget();
        }
        
        if (currentTarget != null)
        {
            AttackTarget();
        }
    }
    
    void FindNewTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float closestDistance = Mathf.Infinity;
        Enemy closestEnemy = null;
        
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.isAlive) continue;
            
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= stats.attackRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        currentTarget = closestEnemy;
    }
    
    void AttackTarget()
    {
        if (currentTarget == null) return;
        
        // Face the target
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        
        // Attack on cooldown
        if (Time.time - lastAttackTime >= stats.attackCooldown)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            if (distance <= stats.attackRange)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }
    
    void PerformAttack()
    {
        if (currentTarget == null || !currentTarget.isAlive) return;
        
        int totalDamage = stats.damage + temporaryDamageBonus;
        currentTarget.TakeDamage(totalDamage);
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Visual feedback
        StartCoroutine(AttackAnimation());
    }
    
    IEnumerator AttackAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.15f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Visual feedback - flash red
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
    
    public void Heal(int amount)
    {
        int maxHealth = stats.health + temporaryHealthBonus;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        StartCoroutine(HealFlash());
        
        Debug.Log($"{stats.unitName} healed for {amount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    IEnumerator HealFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }
    
    void SpawnDogCompanion()
    {
        hasDog = true;
        dogCurrentHealth = stats.dogHealth;
        
        dogCompanion = new GameObject("DogCompanion");
        dogCompanion.transform.parent = transform;
        dogCompanion.transform.localPosition = new Vector3(0.5f, -0.2f, 0);
        
        SpriteRenderer dogSprite = dogCompanion.AddComponent<SpriteRenderer>();
        dogSprite.color = new Color(0.6f, 0.4f, 0.2f);
        dogSprite.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 5;
        
        Debug.Log($"{stats.unitName} has a dog companion!");
    }
    
    void Die()
    {
        isAlive = false;
        GameManager.Instance.UnregisterUnit(this);
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        // Flash red first
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);
        
        // Then fade out
        float duration = 0.5f;
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
    
    public void ApplyTemporaryDamageBuff(int bonus, float duration)
    {
        StartCoroutine(TemporaryDamageBonus(bonus, duration));
    }

    IEnumerator TemporaryDamageBonus(int bonus, float duration)
    {
        temporaryDamageBonus += bonus;
    
        Debug.Log($"{stats.unitName} gained +{bonus} damage!");
    
        yield return new WaitForSeconds(duration);
    
        temporaryDamageBonus -= bonus;
    }
    
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterUnit(this);
        }
    }
}