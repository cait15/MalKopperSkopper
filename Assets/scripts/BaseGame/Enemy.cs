using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ usage

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
    
    [Header("Combat")]
    // NOTE: Replace OfficerUnit with your actual unit script if the name is different
    private OfficerUnit currentTarget; 
    
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    private GameObject towerTarget;

    // --- A* PATHFINDING VARIABLES ---
    private List<PathNode> pathNodeList; // The shortest path sequence calculated by A*
    private int currentPathIndex = 0;
    private const float nodeThreshold = 0.1f; // How close enemy must get to the node
    private PathNode currentTargetNode; // The node we are currently moving toward
    private PathNode towerNode; // The PathNode object closest to the Tower
    // --------------------------------

    // For MeleeV2WithDog
    private GameObject dogCompanion;
    private SpriteRenderer dogSprite;
    private int dogHealth;
    private bool dogAlive = true;
    
    void Start()
    {
        currentHealth = health;
        
        // Find the tower target object (for final movement/damage)
        towerTarget = GameObject.FindGameObjectWithTag("Tower");
        if (towerTarget == null)
        {
            Debug.LogError("No tower found with 'Tower' tag!");
        }

        // Find the final PathNode near the tower
        GameObject towerNodeObject = GameObject.FindGameObjectWithTag("TowerNode");
        if (towerNodeObject != null)
        {
            towerNode = towerNodeObject.GetComponent<PathNode>();
        }
        else
        {
            Debug.LogError("TowerNode object with PathNode script and 'TowerNode' tag not found. Pathfinding will fail.");
        }

        // --- A* PATH CALCULATION ---
        PathNode startNode = FindNearestStartNode();
        
        if (startNode != null && towerNode != null)
        {
            // Calculate the full shortest path using A*
            pathNodeList = AStarPathfinder.FindPath(startNode, towerNode);

            if (pathNodeList != null && pathNodeList.Count > 0)
            {
                currentTargetNode = pathNodeList[currentPathIndex];
            }
        }
        // ---------------------------
        
        // Setup dog for MeleeV2
        if (enemyType == EnemyType.MeleeV2WithDog)
        {
            SpawnDogCompanion();
            dogHealth = 75;
        }
    }

    // Finds the PathNode closest to the enemy's spawn position
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
        
        // Check for nearby officers to attack
        if (currentTarget == null || !currentTarget.isAlive)
        {
            FindNearbyOfficer();
        }
        
        if (currentTarget != null)
        {
            // Enemy stops and attacks the officer
            AttackOfficer();
        }
        else
        {
            // Enemy moves along the calculated A* path
            MoveAlongPath();
        }
    }
    
    // The core movement loop, replacing the old MoveTowardsTower()
    void MoveAlongPath()
    {
        if (currentTargetNode == null)
        {
            // If the path is complete (currentTargetNode is null), move directly to the tower
            MoveTowardsTowerFinalStep(); 
            return;
        }

        // Move towards the current target node
        Vector2 direction = (currentTargetNode.WorldPosition - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        
        // Check if reached current node
        if (Vector2.Distance(transform.position, currentTargetNode.WorldPosition) < nodeThreshold)
        {
            currentPathIndex++;
            
            if (pathNodeList != null && currentPathIndex < pathNodeList.Count)
            {
                // Move to the next node in the calculated A* path
                currentTargetNode = pathNodeList[currentPathIndex];
            }
            else
            {
                // Path is finished (reached the TowerNode), now head to the actual Tower
                currentTargetNode = null;
            }
        }
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", currentTarget == null);
        }
    }

    // Handles the final step toward the actual Tower object
    void MoveTowardsTowerFinalStep()
    {
        if (towerTarget == null)
        {
            Debug.LogWarning("Enemy finished path, but no tower target found!");
            return;
        }
        
        Vector2 direction = (towerTarget.transform.position - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Check if reached tower
        if (Vector2.Distance(transform.position, towerTarget.transform.position) < 1f)
        {
            ReachTower();
        }
    }
    
    // NOTE: This can be called after an ally is defeated to find a new path
    void RecalculatePath()
    {
        PathNode startNode = FindNearestStartNode();
        if (startNode != null && towerNode != null)
        {
            pathNodeList = AStarPathfinder.FindPath(startNode, towerNode);
            currentPathIndex = 0; // Reset index for the new path
            if (pathNodeList != null && pathNodeList.Count > 0)
            {
                currentTargetNode = pathNodeList[currentPathIndex];
            }
        }
    }

    // --- COMBAT LOGIC (Remains the same) ---
    void FindNearbyOfficer()
    {
        // NOTE: Ensure OfficerUnit script exists
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
            // currentTarget.TakeDamage(damage); 
            
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
            spriteRenderer.color = originalColor;
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
        dogSprite.color = new Color(0.6f, 0.4f, 0.2f); 
        dogSprite.sortingLayerName = "Units";
        dogSprite.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 5;
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
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }
        
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damage);
        }
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyReachedEnd();
        }
        
        Destroy(gameObject);
    }
}