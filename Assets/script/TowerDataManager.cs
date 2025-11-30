using UnityEngine;

/// <summary>
/// Quản lý dữ liệu tower upgrade và player data
/// </summary>
public class TowerDataManager : MonoBehaviour
{
    private const string TOWER_DATA_FILE = "TowerUpgradeData";
    private const string PLAYER_DATA_FILE = "PlayerData";

    private static TowerDataManager instance;
    public static TowerDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TowerDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TowerDataManager");
                    instance = go.AddComponent<TowerDataManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private TowerUpgradeData towerData;
    private PlayerData playerData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllData();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load tất cả dữ liệu
    /// </summary>
    private void LoadAllData()
    {
        LoadTowerData();
        LoadPlayerData();
    }

    /// <summary>
    /// Load tower upgrade data
    /// </summary>
    private void LoadTowerData()
    {
        towerData = LocalDataManager.Load<TowerUpgradeData>(TOWER_DATA_FILE);
        if (towerData == null)
        {
            // Tạo dữ liệu mặc định
            towerData = new TowerUpgradeData(0.2f, 5f, 1f, 1f, 1f, 0.1f);
            SaveTowerData();
        }
    }

    /// <summary>
    /// Load player data
    /// </summary>
    private void LoadPlayerData()
    {
        playerData = LocalDataManager.Load<PlayerData>(PLAYER_DATA_FILE);
        if (playerData == null)
        {
            playerData = new PlayerData();
            SavePlayerData();
        }
    }

    /// <summary>
    /// Save tower upgrade data
    /// </summary>
    public void SaveTowerData()
    {
        if (towerData != null)
        {
            LocalDataManager.Save(towerData, TOWER_DATA_FILE);
        }
    }

    /// <summary>
    /// Save player data
    /// </summary>
    public void SavePlayerData()
    {
        if (playerData != null)
        {
            LocalDataManager.Save(playerData, PLAYER_DATA_FILE);
        }
    }

    /// <summary>
    /// Lấy tower upgrade data
    /// </summary>
    public TowerUpgradeData GetTowerData()
    {
        if (towerData == null)
        {
            LoadTowerData();
        }
        return towerData;
    }

    /// <summary>
    /// Lấy player data
    /// </summary>
    public PlayerData GetPlayerData()
    {
        if (playerData == null)
        {
            LoadPlayerData();
        }
        return playerData;
    }

    /// <summary>
    /// Thêm tiền cho player
    /// </summary>
    public void AddMoney(int amount)
    {
        if (playerData == null)
        {
            LoadPlayerData();
        }
        playerData.money += amount;
        SavePlayerData();

        // Thông báo money đã thay đổi
        if (MoneyDisplayManager.Instance != null)
        {
            MoneyDisplayManager.Instance.NotifyMoneyChanged();
        }
    }

    /// <summary>
    /// Trừ tiền của player
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (playerData == null)
        {
            LoadPlayerData();
        }

        if (playerData.money >= amount)
        {
            playerData.money -= amount;
            SavePlayerData();

            // Thông báo money đã thay đổi
            if (MoneyDisplayManager.Instance != null)
            {
                MoneyDisplayManager.Instance.NotifyMoneyChanged();
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// Lấy số tiền hiện tại
    /// </summary>
    public int GetMoney()
    {
        if (playerData == null)
        {
            LoadPlayerData();
        }
        return playerData.money;
    }

    /// <summary>
    /// Upgrade một stat của tower
    /// </summary>
    public bool UpgradeTowerStat(TowerUpgradeType upgradeType, float upgradeAmount, int cost)
    {
        if (towerData == null)
        {
            LoadTowerData();
        }

        // Kiểm tra đủ tiền không
        if (!SpendMoney(cost))
        {
            return false;
        }

        // Upgrade stat tương ứng
        switch (upgradeType)
        {
            case TowerUpgradeType.BaseDamage:
                towerData.baseDamage += upgradeAmount;
                towerData.baseDamageLevel++;
                break;
            case TowerUpgradeType.BaseRange:
                towerData.baseRange += upgradeAmount;
                towerData.baseRangeLevel++;
                break;
            case TowerUpgradeType.BaseFireRate:
                towerData.baseFireRate -= upgradeAmount; // Giảm fire rate = tăng tốc độ bắn
                towerData.baseFireRateLevel++;
                break;
            case TowerUpgradeType.DamagePerLevel:
                towerData.damagePerLevel += upgradeAmount;
                towerData.damagePerLevelUpgrade++;
                break;
            case TowerUpgradeType.RangePerLevel:
                towerData.rangePerLevel += upgradeAmount;
                towerData.rangePerLevelUpgrade++;
                break;
            case TowerUpgradeType.FireRatePerLevel:
                towerData.fireRatePerLevel += upgradeAmount;
                towerData.fireRatePerLevelUpgrade++;
                break;
        }

        SaveTowerData();
        return true;
    }
}

/// <summary>
/// Enum các loại upgrade tower
/// </summary>
public enum TowerUpgradeType
{
    BaseDamage,
    BaseRange,
    BaseFireRate,
    DamagePerLevel,
    RangePerLevel,
    FireRatePerLevel
}

