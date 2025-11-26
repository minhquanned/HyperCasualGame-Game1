using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component bắn của khối Tetris (tower defense) - 3D
/// Sát thương, phạm vi, tốc độ bắn tăng theo level
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseRange = 5f; // World units trong 3D
    [SerializeField] private float baseFireRate = 1f; // Bắn mỗi X giây
    
    [Header("Level Scaling")]
    [SerializeField] private float damagePerLevel = 5f;
    [SerializeField] private float rangePerLevel = 1f;
    [SerializeField] private float fireRatePerLevel = 0.1f; // Giảm thời gian giữa các lần bắn
    
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer rangeIndicator; // LineRenderer để hiển thị phạm vi
  
    private int level = 1;
    private float currentDamage;
    private float currentRange;
    private float currentFireRate;
    private float lastFireTime = 0f;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private Enemy currentTarget;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }
    
    private void Start()
    {
        LoadStatsFromData();
        UpdateStats();
        UpdateRangeIndicator();
    }
    
    /// <summary>
    /// Load stats từ local data khi tower spawn
    /// </summary>
    private void LoadStatsFromData()
    {
        if (TowerDataManager.Instance != null)
        {
            TowerUpgradeData data = TowerDataManager.Instance.GetTowerData();
            if (data != null)
            {
                baseDamage = data.baseDamage;
                baseRange = data.baseRange;
                baseFireRate = data.baseFireRate;
                damagePerLevel = data.damagePerLevel;
                rangePerLevel = data.rangePerLevel;
                fireRatePerLevel = data.fireRatePerLevel;
            }
        }
    }
    
    private void Update()
    {
        UpdateTarget();
        if (currentTarget != null)
        {
            RotateToTarget();
        }
        
        if (currentTarget != null && Time.time >= lastFireTime + currentFireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }
    
    /// <summary>
    /// Cập nhật stats theo level
    /// </summary>
    private void UpdateStats()
    {
        currentDamage = baseDamage + (level - 1) * damagePerLevel;
        currentRange = baseRange + (level - 1) * rangePerLevel;
        currentFireRate = Mathf.Max(0.1f, baseFireRate - (level - 1) * fireRatePerLevel);
    }
    
    /// <summary>
    /// Cập nhật target (quái vật gần nhất trong phạm vi) - 3D
    /// </summary>
    private void UpdateTarget()
    {
        enemiesInRange.Clear();
        
        // Tìm tất cả quái vật trong phạm vi bằng cách duyệt tất cả Enemy
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Vector3 towerPos = transform.position;
        
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && enemy.IsAlive())
            {
                float distance = Vector3.Distance(towerPos, enemy.transform.position);
                if (distance <= currentRange)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        // Chọn target gần nhất
        if (enemiesInRange.Count > 0)
        {
            Enemy closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (Enemy enemy in enemiesInRange)
            {
                float distance = Vector3.Distance(towerPos, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
            
            currentTarget = closest;
        }
        else
        {
            currentTarget = null;
        }
    }
    
    /// <summary>
    /// Bắn đạn
    /// </summary>
    private void Fire()
    {
        if (currentTarget == null || projectilePrefab == null) return;
        
        // Tạo projectile trong 3D space
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(currentTarget, currentDamage);
        }
    }
    
    /// <summary>
    /// Cập nhật level (khi merge)
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        UpdateStats();
        UpdateRangeIndicator();
    }
    
    /// <summary>
    /// Hiển thị phạm vi bắn (LineRenderer)
    /// </summary>
    private void UpdateRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            int segments = 32;
            rangeIndicator.positionCount = segments + 1;
            rangeIndicator.useWorldSpace = true;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * currentRange;
                float z = Mathf.Sin(angle) * currentRange;
                Vector3 pos = transform.position + new Vector3(x, 0.1f, z); // Nâng lên một chút để nhìn thấy
                rangeIndicator.SetPosition(i, pos);
            }
        }
    }

    private void RotateToTarget()
    {
        if (currentTarget == null) return;

        // Lấy vị trí tower và enemy trong 3D
        Vector3 towerPos = transform.position;
        Vector3 enemyPos = currentTarget.transform.position;

        // Tính hướng (vector)
        Vector3 dir = enemyPos - towerPos;
        dir.y = 0; // Chỉ xoay trên mặt phẳng XZ

        // Xoay tower về phía target
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

}

