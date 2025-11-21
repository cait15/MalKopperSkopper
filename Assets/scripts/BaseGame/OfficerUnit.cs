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
    private float buffEndTime = 0f;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform firePoint;
    
    [Header("Visual Feedback")]
    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    
    // For Melee Officer V2
    private GameObject dogCompanion;
    private int dogCurrentHealth;
    private bool hasDog = false;
    
    void Start()
    {
        currentHealth = stats.health;
        GameManager.Instance.RegisterUnit(this);
        
        // Create dog companion for MeleeOfficerV2
        if (stats.unitType == UnitType.MeleeOfficerV2)
        {
            SpawnDogCompanion();
        }
        
        // Setup health bar if prefab exists
        if (healthBarPrefab != null)
        {
            SetupHealthBar();
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
        
        // Update animations based on state
        UpdateAnimations();
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
        // Face the target
        if (currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            
            // Flip sprite based on direction
            if (spriteRenderer != null)
            {
                if (direction.x < 0)
                    spriteRenderer.flipX = true;
                else if (direction.x > 0)
                    spriteRenderer.flipX = false;
            }
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
        // Simple scale animation
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.15f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Set animation parameters
        bool isAttacking = currentTarget != null;
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsAlive", isAlive);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Visual feedback - flash red
        StartCoroutine(DamageFlash());
        
        // Update health bar
        UpdateHealthBar();
        
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
        
        // Visual feedback - flash green
        StartCoroutine(HealFlash());
        
        // Update health bar
        UpdateHealthBar();
        
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
    
    public void ApplyTemporaryHealthBuff(int bonus, float duration)
    {
        StartCoroutine(TemporaryHealthBonus(bonus, duration));
    }
    
    IEnumerator TemporaryHealthBonus(int bonus, float duration)
    {
        temporaryHealthBonus += bonus;
        currentHealth += bonus;
        
        Debug.Log($"{stats.unitName} gained +{bonus} temporary health!");
        UpdateHealthBar();
        
        yield return new WaitForSeconds(duration);
        
        temporaryHealthBonus -= bonus;
        currentHealth = Mathf.Min(currentHealth, stats.health);
        UpdateHealthBar();
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
    
    void SpawnDogCompanion()
    {
        hasDog = true;
        dogCurrentHealth = stats.dogHealth;
        
        // Create a simple dog sprite
        dogCompanion = new GameObject("DogCompanion");
        dogCompanion.transform.parent = transform;
        dogCompanion.transform.localPosition = new Vector3(0.5f, -0.2f, 0);
        
        // Add sprite renderer
        SpriteRenderer dogSprite = dogCompanion.AddComponent<SpriteRenderer>();
        dogSprite.color = new Color(0.6f, 0.4f, 0.2f); // Brown color
        dogSprite.sortingOrder = spriteRenderer.sortingOrder;
        
        Debug.Log($"{stats.unitName} has a dog companion!");
    }
    
    void SetupHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            // Update health bar fill amount
            UnityEngine.UI.Image fillImage = healthBarInstance.GetComponentInChildren<UnityEngine.UI.Image>();
            if (fillImage != null)
            {
                int maxHealth = stats.health + temporaryHealthBonus;
                fillImage.fillAmount = (float)currentHealth / maxHealth;
            }
        }
    }
    
    void Die()
    {
        isAlive = false;
        GameManager.Instance.UnregisterUnit(this);
        
        // Notify InputManager to free up the placement spot
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.OnUnitDestroyed(this);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Death animation
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
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
    
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterUnit(this);
        }
    }
    
    // Helper method to visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats != null ? stats.attackRange : 2f);
    }
}