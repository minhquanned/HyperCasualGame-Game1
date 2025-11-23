using UnityEngine;

/// <summary>
/// Đạn bắn từ tower (3D)
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f; // World units per second trong 3D
    [SerializeField] private float lifetime = 5f;
    
    private Enemy target;
    private float damage;
    private float spawnTime;
    
    public void Initialize(Enemy targetEnemy, float projectileDamage)
    {
        target = targetEnemy;
        damage = projectileDamage;
        spawnTime = Time.time;
    }
    
    private void Update()
    {
        // Nếu target còn sống, di chuyển về phía target
        if (target != null && target.IsAlive())
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // Xoay projectile về phía target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Kiểm tra va chạm
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < 0.2f) // Threshold cho 3D
            {
                HitTarget();
            }
        }
        else
        {
            // Target đã chết, tự hủy
            Destroy(gameObject);
        }
        
        // Tự hủy sau một thời gian
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}

