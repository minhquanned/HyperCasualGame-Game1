using System;

/// <summary>
/// Dữ liệu nâng cấp tower - lưu 6 giá trị từ Tower.cs
/// </summary>
[Serializable]
public class TowerUpgradeData
{
    public float baseDamage = 10f;
    public float baseRange = 5f;
    public float baseFireRate = 1f;
    public float damagePerLevel = 5f;
    public float rangePerLevel = 1f;
    public float fireRatePerLevel = 0.1f;
    
    // Level upgrade cho mỗi stat (0 = chưa upgrade)
    public int baseDamageLevel = 0;
    public int baseRangeLevel = 0;
    public int baseFireRateLevel = 0;
    public int damagePerLevelUpgrade = 0;
    public int rangePerLevelUpgrade = 0;
    public int fireRatePerLevelUpgrade = 0;
    
    public TowerUpgradeData()
    {
        // Giá trị mặc định
    }
    
    public TowerUpgradeData(float baseDmg, float baseRng, float baseFR, 
                           float dmgPerLvl, float rngPerLvl, float frPerLvl)
    {
        baseDamage = baseDmg;
        baseRange = baseRng;
        baseFireRate = baseFR;
        damagePerLevel = dmgPerLvl;
        rangePerLevel = rngPerLvl;
        fireRatePerLevel = frPerLvl;
    }
}

