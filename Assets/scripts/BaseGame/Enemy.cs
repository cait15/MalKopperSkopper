using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyType
{
    MeleeV1,
    Tank,
    Ranged,
    Boss
}

public class Enemy : MonoBehaviour
{
    [Header("MiniBoss Configuration")]
    public bool isMiniBoss = false;
    public float miniBossHealthMultiplier = 3.0f; 
    public float miniBossDamageMultiplier = 2.0f;
    public float miniBossSpeedMultiplier = 2.0f;
    
    [Header("Enemy Type")]
    public EnemyType enemyType;
    
    [Header("Enemy Stats")]
    public int health = 50;
    public float speed = 2f;
    public int damage = 10;
    public int moneyReward = 50;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    
    [Header("Bullet")]
    public Sprite bulletSprite;
    public float bulletScale = 0.8f;
    
    [Header("State")]
    public bool isAlive = true;
    private int currentHealth;
    private float lastAttackTime;
    
    [Header("Combat")]
    private OfficerUnit currentTarget; 
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    private GameObject towerTarget;
    private List<PathNode> pathNodeList;
    private int currentPathIndex = 0;
    private const float nodeThreshold = 0.1f;
    private PathNode currentTargetNode;
    private PathNode towerNode;
    
    void Start()
    {
        if (isMiniBoss)
        {
            health = Mathf.RoundToInt(health * miniBossHealthMultiplier);
            damage = Mathf.RoundToInt(damage * miniBossDamageMultiplier);
        }

        currentHealth = health;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        towerTarget = GameObject.FindGameObjectWithTag("Tower");
        if (towerTarget == null)
        {
            Debug.LogError("No tower found with 'Tower' tag!");
        }

        GameObject towerNodeObject = GameObject.FindGameObjectWithTag("TowerNode");
        if (towerNodeObject != null)
        {
            towerNode = towerNodeObject.GetComponent<PathNode>();
        }
        else
        {
            Debug.LogError("TowerNode object with PathNode script and 'TowerNode' tag not found.");
        }

        PathNode startNode = FindNearestStartNode();
        
        if (startNode != null && towerNode != null)
        {
            pathNodeList = AStarPathfinder.FindPath(startNode, towerNode);

            if (pathNodeList != null && pathNodeList.Count > 0)
            {
                currentTargetNode = pathNodeList[currentPathIndex];
            }
        }
    }

    private PathNode FindNearestStartNode()
    {
        PathNode[] allNodes = FindObjectsOfType<PathNode>();
        PathNode nearestNode = null;
        float shortestDistance = float.MaxValue;
        
        foreach(PathNode node in allNodes)
        {
            float distance = Vector3.Distance(transform.position, node.WorldPosition);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestNode = node;
            }
        }
        return nearestNode;
    }

    void Update()
    {
        if (!isAlive) return;
        
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
        if (currentTargetNode == null)
        {
            MoveTowardsTowerFinalStep(); 
            return;
        }

        Vector3 direction = (currentTargetNode.WorldPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        
        if (Vector3.Distance(transform.position, currentTargetNode.WorldPosition) < nodeThreshold)
        {
            currentPathIndex++;
            
            if (pathNodeList != null && currentPathIndex < pathNodeList.Count)
            {
                currentTargetNode = pathNodeList[currentPathIndex];
            }
            else
            {
                currentTargetNode = null;
            }
        }
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", currentTarget == null);
        }
    }

    void MoveTowardsTowerFinalStep()
    {
        if (towerTarget == null)
        {
            Debug.LogWarning("Enemy finished path, but no tower target found!");
            return;
        }
        
        Vector3 direction = (towerTarget.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, towerTarget.transform.position) < 1f)
        {
            ReachTower();
        }
    }
    
    void RecalculatePath()
    {
        PathNode startNode = FindNearestStartNode();
        if (startNode != null && towerNode != null)
        {
            pathNodeList = AStarPathfinder.FindPath(startNode, towerNode);
            currentPathIndex = 0;
            if (pathNodeList != null && pathNodeList.Count > 0)
            {
                currentTargetNode = pathNodeList[currentPathIndex];
            }
        }
    }

    void FindNearbyOfficer()
    {
        OfficerUnit[] officers = FindObjectsOfType<OfficerUnit>();
        float closestDistance = float.MaxValue;
        OfficerUnit closestOfficer = null;
    
        foreach (OfficerUnit officer in officers)
        {
            if (!officer.isAlive) continue;
        
            float distance = Vector3.Distance(transform.position, officer.transform.position);
            if (distance < closestDistance && distance <= attackRange)
            {
                closestDistance = distance;
                closestOfficer = officer;
            }
        }
    
        currentTarget = closestOfficer;
    }
    
    void AttackOfficer()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            
            if (spriteRenderer != null)
            {
                if (direction.x < 0)
                    spriteRenderer.flipX = true;
                else if (direction.x > 0)
                    spriteRenderer.flipX = false;
            }
        }
        
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
            if (enemyType == EnemyType.Ranged)
            {
                FireBullet(damage);
            }
            else
            {
                currentTarget.TakeDamage(damage);
            }
            
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
    
    void FireBullet(int bulletDamage)
    {
        GameObject bulletObj = new GameObject("EnemyBullet");
        bulletObj.transform.position = transform.position;
        bulletObj.transform.localScale = Vector3.one * bulletScale;
        
        SpriteRenderer bulletSpriteRenderer = bulletObj.AddComponent<SpriteRenderer>();
        Debug.Log("Bullet fired!");
        if (bulletSprite != null)
        {
            bulletSpriteRenderer.sprite = bulletSprite;
        }
        bulletSpriteRenderer.color = Color.red;
        bulletSpriteRenderer.sortingLayerName = "Units";
        bulletSpriteRenderer.sortingOrder = 10;
        
        SphereCollider collider = bulletObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
        
        Rigidbody rb = bulletObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.damage = bulletDamage;
        bullet.isAllyBullet = false;
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
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
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
    
    void Die()
    {
        isAlive = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }
        else if (TutGameManager.Instance != null)
        {
            TutGameManager.Instance.AddMoney(moneyReward);
        }
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyKilled();
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
            
            transform.localScale = Vector3.Lerp(Vector3.one * 0.05f, Vector3.zero, t);
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void ReachTower()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damage);
        }
        else if (TutGameManager.Instance != null)
        {
            TutGameManager.Instance.TakeDamage(damage);
        }
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyReachedEnd();
        }
        
        Destroy(gameObject);
    }
}