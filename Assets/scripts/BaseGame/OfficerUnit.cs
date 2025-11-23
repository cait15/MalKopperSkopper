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
    private float lastSearchTime;
    private float searchCooldown = 0.5f;
    
    [Header("Buffs")]
    public int temporaryHealthBonus = 0;
    public int temporaryDamageBonus = 0;
    
    [Header("Bullet")]
    public Sprite bulletSprite;
    public float bulletScale = 0.1f;
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public HealthBar healthBar;
    
    void Start()
    {
        if (stats == null)
        {
            Debug.LogError("OfficerUnit has no stats assigned!");
            return;
        }
        
        currentHealth = stats.health;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUnit(this);
        }
        else if (TutGameManager.Instance != null)
        {
            TutGameManager.Instance.RegisterUnit(this);
        }
        
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }
        
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(stats.health, currentHealth);
        }
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        if (currentTarget == null || !currentTarget.isAlive)
        {
            if (Time.time - lastSearchTime >= searchCooldown)
            {
                FindNewTarget();
                lastSearchTime = Time.time;
            }
        }
        
        if (currentTarget != null)
        {
            AttackTarget();
        }
    }
    
    void FindNewTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;
        
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.isAlive) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= stats.attackRange)
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
        
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        
        if (Time.time - lastAttackTime >= stats.attackCooldown)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            
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
        
        if (stats.unitType == UnitType.RangedOfficer)
        {
            FireBullet(totalDamage);
        }
        else
        {
            currentTarget.TakeDamage(totalDamage);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        StartCoroutine(AttackAnimation());
    }
    
    void FireBullet(int bulletDamage)
    {
        GameObject bulletObj = new GameObject("AllyBullet");
        bulletObj.transform.position = transform.position;
        bulletObj.transform.localScale = Vector3.one * bulletScale;
        
        SpriteRenderer bulletSpriteRenderer = bulletObj.AddComponent<SpriteRenderer>();
        if (bulletSprite != null)
        {
            bulletSpriteRenderer.sprite = bulletSprite;
        }
        bulletSpriteRenderer.color = Color.green;
        bulletSpriteRenderer.sortingLayerName = "Units";
        bulletSpriteRenderer.sortingOrder = 10;
        
        SphereCollider collider = bulletObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
        
        Rigidbody rb = bulletObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.damage = bulletDamage;
        bullet.isAllyBullet = true;
        bullet.speed = 15f;
        
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        bullet.SetDirection(direction);
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
        
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(stats.health + temporaryHealthBonus, currentHealth);
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
    
    public void Heal(int amount)
    {
        int maxHealth = stats.health + temporaryHealthBonus;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(maxHealth, currentHealth);
        }
        
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
    
    void Die()
    {
        isAlive = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterUnit(this);
        }
        else if (TutGameManager.Instance != null)
        {
            TutGameManager.Instance.UnregisterUnit(this);
        }
        
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.ClearPlacementForUnit(this);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);
        
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
            transform.localScale = Vector3.Lerp(Vector3.one * 0.05f, Vector3.zero, t);
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
        else if (TutGameManager.Instance != null)
        {
            TutGameManager.Instance.UnregisterUnit(this);
        }
    }
}