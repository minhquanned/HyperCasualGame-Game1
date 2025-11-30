using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI để nâng cấp tower ở main menu
/// </summary>
public class UIUpgradeTower : MonoBehaviour
{
    [Header("Upgrade Buttons - Base Stats")]
    [SerializeField] private Button upgradeBaseDamageButton;
    [SerializeField] private TextMeshProUGUI baseDamageText;
    [SerializeField] private TextMeshProUGUI baseDamageCostText;

    [SerializeField] private Button upgradeBaseRangeButton;
    [SerializeField] private TextMeshProUGUI baseRangeText;
    [SerializeField] private TextMeshProUGUI baseRangeCostText;

    [SerializeField] private Button upgradeBaseFireRateButton;
    [SerializeField] private TextMeshProUGUI baseFireRateText;
    [SerializeField] private TextMeshProUGUI baseFireRateCostText;

    [Header("Upgrade Buttons - Per Level Stats")]
    [SerializeField] private Button upgradeDamagePerLevelButton;
    [SerializeField] private TextMeshProUGUI damagePerLevelText;
    [SerializeField] private TextMeshProUGUI damagePerLevelCostText;

    [SerializeField] private Button upgradeRangePerLevelButton;
    [SerializeField] private TextMeshProUGUI rangePerLevelText;
    [SerializeField] private TextMeshProUGUI rangePerLevelCostText;

    [SerializeField] private Button upgradeFireRatePerLevelButton;
    [SerializeField] private TextMeshProUGUI fireRatePerLevelText;
    [SerializeField] private TextMeshProUGUI fireRatePerLevelCostText;

    [Header("Upgrade Settings")]
    [SerializeField] private float baseDamageUpgrade = 5f;
    [SerializeField] private float baseRangeUpgrade = 1f;
    [SerializeField] private float baseFireRateUpgrade = 0.1f;
    [SerializeField] private float damagePerLevelUpgrade = 1f;
    [SerializeField] private float rangePerLevelUpgrade = 0.2f;
    [SerializeField] private float fireRatePerLevelUpgrade = 0.02f;

    [SerializeField] private int baseCost = 100;
    [SerializeField] private float costMultiplier = 1.5f; // Mỗi lần upgrade, cost tăng 1.5x

    private TowerDataManager dataManager;
    private TowerUpgradeData towerData;
    private MoneyDisplayManager moneyManager;

    private void Start()
    {
        dataManager = TowerDataManager.Instance;
        moneyManager = MoneyDisplayManager.Instance;

        if (dataManager == null)
        {
            Debug.LogError("TowerDataManager not found!");
            return;
        }

        towerData = dataManager.GetTowerData();

        // Setup button listeners
        if (upgradeBaseDamageButton != null)
            upgradeBaseDamageButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.BaseDamage, baseDamageUpgrade));

        if (upgradeBaseRangeButton != null)
            upgradeBaseRangeButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.BaseRange, baseRangeUpgrade));

        if (upgradeBaseFireRateButton != null)
            upgradeBaseFireRateButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.BaseFireRate, baseFireRateUpgrade));

        if (upgradeDamagePerLevelButton != null)
            upgradeDamagePerLevelButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.DamagePerLevel, damagePerLevelUpgrade));

        if (upgradeRangePerLevelButton != null)
            upgradeRangePerLevelButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.RangePerLevel, rangePerLevelUpgrade));

        if (upgradeFireRatePerLevelButton != null)
            upgradeFireRatePerLevelButton.onClick.AddListener(() => UpgradeStat(TowerUpgradeType.FireRatePerLevel, fireRatePerLevelUpgrade));

        UpdateUI();
    }

    private void OnEnable()
    {
        if (dataManager != null)
        {
            towerData = dataManager.GetTowerData();
            UpdateUI();
        }
    }

    /// <summary>
    /// Upgrade một stat
    /// </summary>
    private void UpgradeStat(TowerUpgradeType upgradeType, float upgradeAmount)
    {
        if (dataManager == null || towerData == null) return;

        int currentLevel = GetCurrentLevel(upgradeType);
        int cost = CalculateCost(currentLevel);

        if (dataManager.UpgradeTowerStat(upgradeType, upgradeAmount, cost))
        {
            towerData = dataManager.GetTowerData();
            UpdateUI();
        }
        else
        {
            Debug.Log("Đủ tiền để nâng cấp!");
        }
    }

    /// <summary>
    /// Tính cost dựa trên level hiện tại
    /// </summary>
    private int CalculateCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    /// <summary>
    /// Lấy level hiện tại của một stat
    /// </summary>
    private int GetCurrentLevel(TowerUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case TowerUpgradeType.BaseDamage:
                return towerData.baseDamageLevel;
            case TowerUpgradeType.BaseRange:
                return towerData.baseRangeLevel;
            case TowerUpgradeType.BaseFireRate:
                return towerData.baseFireRateLevel;
            case TowerUpgradeType.DamagePerLevel:
                return towerData.damagePerLevelUpgrade;
            case TowerUpgradeType.RangePerLevel:
                return towerData.rangePerLevelUpgrade;
            case TowerUpgradeType.FireRatePerLevel:
                return towerData.fireRatePerLevelUpgrade;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Cập nhật toàn bộ UI
    /// </summary>
    private void UpdateUI()
    {
        if (dataManager == null || towerData == null) return;

        // Cập nhật Base Damage
        if (baseDamageText != null)
        {
            baseDamageText.text = $"Damage: {towerData.baseDamage:F1}";
        }
        if (baseDamageCostText != null)
        {
            int cost = CalculateCost(towerData.baseDamageLevel);
            baseDamageCostText.text = $"{cost}";
        }
        if (upgradeBaseDamageButton != null)
        {
            int cost = CalculateCost(towerData.baseDamageLevel);
            upgradeBaseDamageButton.interactable = dataManager.GetMoney() >= cost;
        }

        // Cập nhật Base Range
        if (baseRangeText != null)
        {
            baseRangeText.text = $"Range: {towerData.baseRange:F1}";
        }
        if (baseRangeCostText != null)
        {
            int cost = CalculateCost(towerData.baseRangeLevel);
            baseRangeCostText.text = $"{cost}";
        }
        if (upgradeBaseRangeButton != null)
        {
            int cost = CalculateCost(towerData.baseRangeLevel);
            upgradeBaseRangeButton.interactable = dataManager.GetMoney() >= cost;
        }

        // Cập nhật Base Fire Rate
        if (baseFireRateText != null)
        {
            baseFireRateText.text = $"Fire Rate: {towerData.baseFireRate:F2}s";
        }
        if (baseFireRateCostText != null)
        {
            int cost = CalculateCost(towerData.baseFireRateLevel);
            baseFireRateCostText.text = $"{cost}";
        }
        if (upgradeBaseFireRateButton != null)
        {
            int cost = CalculateCost(towerData.baseFireRateLevel);
            upgradeBaseFireRateButton.interactable = dataManager.GetMoney() >= cost;
        }

        // Cập nhật Damage Per Level
        if (damagePerLevelText != null)
        {
            damagePerLevelText.text = $"Dmg/Level: +{towerData.damagePerLevel:F1}";
        }
        if (damagePerLevelCostText != null)
        {
            int cost = CalculateCost(towerData.damagePerLevelUpgrade);
            damagePerLevelCostText.text = $"{cost}";
        }
        if (upgradeDamagePerLevelButton != null)
        {
            int cost = CalculateCost(towerData.damagePerLevelUpgrade);
            upgradeDamagePerLevelButton.interactable = dataManager.GetMoney() >= cost;
        }

        // Cập nhật Range Per Level
        if (rangePerLevelText != null)
        {
            rangePerLevelText.text = $"Range/Level: +{towerData.rangePerLevel:F1}";
        }
        if (rangePerLevelCostText != null)
        {
            int cost = CalculateCost(towerData.rangePerLevelUpgrade);
            rangePerLevelCostText.text = $"{cost}";
        }
        if (upgradeRangePerLevelButton != null)
        {
            int cost = CalculateCost(towerData.rangePerLevelUpgrade);
            upgradeRangePerLevelButton.interactable = dataManager.GetMoney() >= cost;
        }

        // Cập nhật Fire Rate Per Level
        if (fireRatePerLevelText != null)
        {
            fireRatePerLevelText.text = $"FR/Level: +{towerData.fireRatePerLevel:F2}s";
        }
        if (fireRatePerLevelCostText != null)
        {
            int cost = CalculateCost(towerData.fireRatePerLevelUpgrade);
            fireRatePerLevelCostText.text = $"{cost}";
        }
        if (upgradeFireRatePerLevelButton != null)
        {
            int cost = CalculateCost(towerData.fireRatePerLevelUpgrade);
            upgradeFireRatePerLevelButton.interactable = dataManager.GetMoney() >= cost;
        }
    }

    /// <summary>
    /// Public method để refresh UI từ bên ngoài
    /// </summary>
    public void RefreshUI()
    {
        if (dataManager != null)
        {
            towerData = dataManager.GetTowerData();
            UpdateUI();
        }
    }
}

