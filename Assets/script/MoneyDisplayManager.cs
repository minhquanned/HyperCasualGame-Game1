using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý hiển thị money cho toàn bộ các UI trong scene
/// Sử dụng event-based system để cập nhật khi có biến động
/// Tự động tìm các TextMeshProUGUI theo tag
/// </summary>
public class MoneyDisplayManager : MonoBehaviour
{
    public static MoneyDisplayManager Instance { get; private set; }

    [Header("Auto-detect Settings")]
    [SerializeField] private string moneyDisplayTag = "MoneyDisplay";
    [Tooltip("Nếu true, tự động tìm các TextMeshProUGUI có tag khi Start")]
    [SerializeField] private bool autoDetectOnStart = true;
    [Tooltip("Nếu true, tự động scan lại mỗi khi scene có object mới được enable")]
    [SerializeField] private bool autoDetectNewObjects = true;

    // Event được gọi khi money thay đổi
    public event System.Action<int> OnMoneyChanged;

    private List<TextMeshProUGUI> moneyDisplayTexts = new List<TextMeshProUGUI>();
    private TowerDataManager dataManager;
    private int currentMoney;

    private void Awake()
    {
        // Singleton pattern cho scene (không persist qua scene)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        dataManager = TowerDataManager.Instance;
        if (dataManager == null)
        {
            Debug.LogError("TowerDataManager not found!");
            return;
        }

        // Lấy giá trị money ban đầu
        currentMoney = dataManager.GetMoney();

        // Tự động tìm các money display theo tag
        if (autoDetectOnStart)
        {
            FindAndRegisterAllMoneyDisplays();
        }

        UpdateAllDisplays();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Tự động tìm và đăng ký tất cả TextMeshProUGUI có tag MoneyDisplay
    /// </summary>
    public void FindAndRegisterAllMoneyDisplays()
    {
        // Tìm tất cả GameObject có tag
        GameObject[] moneyObjects = GameObject.FindGameObjectsWithTag(moneyDisplayTag);

        int registeredCount = 0;
        foreach (GameObject obj in moneyObjects)
        {
            // Lấy TextMeshProUGUI component
            TextMeshProUGUI textComponent = obj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                RegisterMoneyDisplay(textComponent);
                registeredCount++;
            }
            else
            {
                // Thử tìm trong children
                TextMeshProUGUI childText = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (childText != null)
                {
                    RegisterMoneyDisplay(childText);
                    registeredCount++;
                }
            }
        }

        if (registeredCount > 0)
        {
            Debug.Log($"MoneyDisplayManager: Đã đăng ký {registeredCount} money displays");
        }
    }

    /// <summary>
    /// Đăng ký một TextMeshProUGUI để hiển thị money
    /// </summary>
    public void RegisterMoneyDisplay(TextMeshProUGUI displayText)
    {
        if (displayText != null && !moneyDisplayTexts.Contains(displayText))
        {
            moneyDisplayTexts.Add(displayText);
            UpdateSingleDisplay(displayText);
        }
    }

    /// <summary>
    /// Hủy đăng ký một TextMeshProUGUI
    /// </summary>
    public void UnregisterMoneyDisplay(TextMeshProUGUI displayText)
    {
        if (displayText != null && moneyDisplayTexts.Contains(displayText))
        {
            moneyDisplayTexts.Remove(displayText);
        }
    }

    /// <summary>
    /// Thông báo money đã thay đổi (gọi từ các script khác khi money thay đổi)
    /// </summary>
    public void NotifyMoneyChanged()
    {
        if (dataManager == null) return;

        int newMoney = dataManager.GetMoney();
        if (newMoney != currentMoney)
        {
            currentMoney = newMoney;

            // Tự động tìm thêm displays mới nếu được bật
            if (autoDetectNewObjects)
            {
                FindAndRegisterAllMoneyDisplays();
            }

            UpdateAllDisplays();
            OnMoneyChanged?.Invoke(currentMoney);
        }
    }

    /// <summary>
    /// Cập nhật tất cả các UI hiển thị money
    /// </summary>
    private void UpdateAllDisplays()
    {
        // Xóa các reference null
        moneyDisplayTexts.RemoveAll(item => item == null);

        foreach (var displayText in moneyDisplayTexts)
        {
            UpdateSingleDisplay(displayText);
        }
    }

    /// <summary>
    /// Cập nhật một UI hiển thị money
    /// </summary>
    private void UpdateSingleDisplay(TextMeshProUGUI displayText)
    {
        if (displayText != null)
        {
            displayText.text = $"{currentMoney}";
        }
    }

    /// <summary>
    /// Lấy giá trị money hiện tại
    /// </summary>
    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    /// <summary>
    /// Force update tất cả displays (dùng khi cần refresh)
    /// </summary>
    public void ForceUpdateAll()
    {
        if (dataManager != null)
        {
            currentMoney = dataManager.GetMoney();
            UpdateAllDisplays();
        }
    }
}
