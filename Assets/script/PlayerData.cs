using System;

/// <summary>
/// Dữ liệu player - lưu tiền và các thông tin khác
/// </summary>
[Serializable]
public class PlayerData
{
    public int money = 0;
    
    public PlayerData()
    {
        money = 0;
    }
}

