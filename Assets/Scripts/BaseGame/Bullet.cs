// ============ BULLET SCRIPT (Standalone) ============
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 15f;
    public int damage = 15;
    public bool isAllyBullet = true;
    
    private Vector3 direction;
    private SpriteRenderer spriteRenderer;
    private float maxDistance = 100f;
    private Vector3 startPos;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }
    
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        
        if (Vector3.Distance(transform.position, startPos) > maxDistance)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
        // For isometric: rotate around Y axis based on X and Z direction
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
    
    void OnTriggerEnter(Collider collision)
    {
        if (isAllyBullet)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.isAlive)
            {
                enemy.TakeDamage(damage);
                OnHit();
            }
        }
        else
        {
            OfficerUnit unit = collision.GetComponent<OfficerUnit>();
            if (unit != null && unit.isAlive)
            {
                unit.TakeDamage(damage);
                OnHit();
            }
        }
    }
    
    void OnHit()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(HitAnimation());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    System.Collections.IEnumerator HitAnimation()
    {
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
