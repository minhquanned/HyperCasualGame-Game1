using UnityEngine;
using System.Collections.Generic;
using System;

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
    [SerializeField] private Transform gunBarrel; // Nòng súng - chỉ xoay object này
    [SerializeField] private List<Renderer> towerRenderers = new List<Renderer>(); // Danh sách renderer của model tower

    private int level = 1;
    private float currentDamage;
    private float currentRange;
    private float currentFireRate;
    private float lastFireTime = 0f;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private Enemy currentTarget;
    
    // Cache material instances để tránh tạo mới mỗi lần (chỉ cache material "UpgradeMat")
    private Dictionary<Renderer, Material> cachedUpgradeMaterials = new Dictionary<Renderer, Material>();

    // Màu sắc theo rarity level
    private static readonly Dictionary<int, Color> RarityColors = new Dictionary<int, Color>
    {
        { 1, HexToColor("fdfefe") }, // Common
        { 2, HexToColor("27AE60") }, // Uncommon
        { 3, HexToColor("2471A3") }, // Rare
        { 4, HexToColor("7D3C98") }, // Epic
        { 5, HexToColor("f1c40f") }, // Legendary
        { 6, HexToColor("D35400") }, // Mythic
        { 7, HexToColor("7B241C") }, // Relic
        { 8, HexToColor("B3CA1F") }, // Masterwork
        { 9, HexToColor("dc2367") }  // Eternal
    };

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
        UpdateTowerColor();
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
        UpdateTowerColor();
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

        // Nếu không có gunBarrel được assign, không làm gì
        if (gunBarrel == null) return;

        // Lấy vị trí gunBarrel và enemy trong 3D
        Vector3 gunPos = gunBarrel.position;
        Vector3 enemyPos = currentTarget.transform.position;

        // Tính hướng (vector)
        Vector3 dir = enemyPos - gunPos;
        dir.y = 0; // Chỉ xoay trên mặt phẳng XZ

        // Xoay nòng súng về phía target - chỉ rotation Y
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            // Giữ nguyên X và Z, chỉ thay đổi Y
            Vector3 currentEuler = gunBarrel.eulerAngles;
            gunBarrel.eulerAngles = new Vector3(currentEuler.x, targetRotation.eulerAngles.y, currentEuler.z);
        }
    }

    /// <summary>
    /// Chuyển đổi hex color string sang Color
    /// </summary>
    private static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length != 6)
        {
            Debug.LogWarning($"Invalid hex color: {hex}, using white");
            return Color.white;
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    /// <summary>
    /// Cập nhật màu sắc tháp dựa trên level
    /// Sử dụng cached material instance để đảm bảo đổi màu thành công và tránh memory leak
    /// </summary>
    private void UpdateTowerColor()
    {
        if (towerRenderers == null || towerRenderers.Count == 0) return;

        // Lấy rarity level (1-9) dựa trên level hiện tại
        int rarityLevel = Mathf.Clamp(level, 1, 9);

        if (!RarityColors.ContainsKey(rarityLevel)) return;

        Color rarityColor = RarityColors[rarityLevel];
        
        // Duyệt qua tất cả renderer trong danh sách
        foreach (Renderer renderer in towerRenderers)
        {
            if (renderer == null) continue;

            // Lấy hoặc cache material "UpgradeMat"
            Material upgradeMaterial = GetOrCacheUpgradeMaterial(renderer);
            
            if (upgradeMaterial != null)
            {
                // Thử set màu với nhiều property name phổ biến (URP shader thường dùng _BaseColor)
                if (upgradeMaterial.HasProperty("_BaseColor"))
                {
                    upgradeMaterial.SetColor("_BaseColor", rarityColor);
                }
                else if (upgradeMaterial.HasProperty("_Color"))
                {
                    upgradeMaterial.SetColor("_Color", rarityColor);
                }
                else if (upgradeMaterial.HasProperty("_MainColor"))
                {
                    upgradeMaterial.SetColor("_MainColor", rarityColor);
                }
                else if (upgradeMaterial.HasProperty("_TintColor"))
                {
                    upgradeMaterial.SetColor("_TintColor", rarityColor);
                }
                else
                {
                    // Fallback: thử set _Color dù không có property (một số shader vẫn chấp nhận)
                    upgradeMaterial.color = rarityColor;
                }
            }
        }
    }

    /// <summary>
    /// Lấy hoặc cache material "UpgradeMat" từ renderer
    /// Chỉ tạo instance một lần để tránh memory leak
    /// </summary>
    private Material GetOrCacheUpgradeMaterial(Renderer renderer)
    {
        if (renderer == null) return null;

        // Nếu đã cache rồi, trả về
        if (cachedUpgradeMaterials.ContainsKey(renderer))
        {
            return cachedUpgradeMaterials[renderer];
        }

        // Tìm material có tên "UpgradeMat"
        Material[] materials = renderer.materials;
        int upgradeMatIndex = -1;
        
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].name.Contains("UpgradeMat"))
            {
                upgradeMatIndex = i;
                break;
            }
        }

        if (upgradeMatIndex < 0) return null;

        // Tạo instance của material để có thể chỉnh sửa mà không ảnh hưởng material gốc
        // Chỉ tạo một lần và cache lại
        Material materialInstance = new Material(materials[upgradeMatIndex]);
        cachedUpgradeMaterials[renderer] = materialInstance;
        
        // Cập nhật lại materials array với instance mới
        materials[upgradeMatIndex] = materialInstance;
        renderer.materials = materials;
        
        return materialInstance;
    }

    /// <summary>
    /// Cleanup cached materials khi tower bị destroy
    /// </summary>
    private void OnDestroy()
    {
        // Cleanup cached materials để tránh memory leak
        foreach (var kvp in cachedUpgradeMaterials)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        cachedUpgradeMaterials.Clear();
    }

}

