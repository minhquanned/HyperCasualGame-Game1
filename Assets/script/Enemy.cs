using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quái vật di chuyển theo waypoint và tấn công thành (3D)
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f; // World units per second (3D)
    [SerializeField] private float damageToBase = 10f;

    [Header("Visual")]
    [SerializeField] private GameObject healthBarPrefab; // Prefab cho health bar (3D Canvas hoặc Billboard)
    [SerializeField] private Transform healthBarParent;

    private float currentHealth;
    private List<Vector3> path;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private Base targetBase;

    private void Start()
    {
        currentHealth = maxHealth;
        targetBase = FindFirstObjectByType<Base>();
        UpdateHealthBar();
    }

    /// <summary>
    /// Khởi tạo enemy với đường đi (3D waypoints)
    /// </summary>
    public void Initialize(List<Vector3> waypoints)
    {
        path = new List<Vector3>(waypoints);
        currentWaypointIndex = 0;

        if (path.Count > 0)
        {
            transform.position = path[0];
            isMoving = true;
        }
    }

    private void Update()
    {
        if (isMoving && path != null && path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    /// <summary>
    /// Enemy di chuyển theo waypoint
    /// </summary>
    private void MoveAlongPath()
    {
        if (currentWaypointIndex >= path.Count)
        {
            AttackBase();
            return;
        }

        Vector3 targetWaypoint = path[currentWaypointIndex];
        float distance = Vector3.Distance(transform.position, targetWaypoint);

        if (distance < 0.1f) // Threshold cho 3D
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= path.Count)
            {
                AttackBase();
                return;
            }

            targetWaypoint = path[currentWaypointIndex];
        }

        Vector3 direction = (targetWaypoint - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Xoay enemy về phía đang di chuyển
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Enemy nhận damage
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isMoving = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// Enemy chạm thành
    /// </summary>
    private void AttackBase()
    {
        if (targetBase != null)
            targetBase.TakeDamage(damageToBase);

        Destroy(gameObject);
    }

    /// <summary>
    /// Cập nhật thanh máu (3D)
    /// </summary>
    private void UpdateHealthBar()
    {
        // Có thể dùng 3D Canvas hoặc Billboard để hiển thị health bar
        // Hoặc dùng TextMeshPro để hiển thị số HP
        // Tùy vào cách bạn muốn implement
    }

    /// <summary>
    /// Set stats cho enemy khi spawn (đã thêm speed)
    /// </summary>
    public void SetStats(float health, float damage, float speed)
    {
        maxHealth = health;
        currentHealth = health;
        damageToBase = damage;
        moveSpeed = speed;

        UpdateHealthBar();
    }

    /// <summary>
    /// Kiểm tra sống hay chết
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}
