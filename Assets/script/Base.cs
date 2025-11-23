using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Thành - có HP, game over khi HP = 0
/// </summary>
public class Base : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Visual")]
    [SerializeField] private TMPro.TextMeshPro healthText; // 3D text
    // Health bar có thể dùng 3D Canvas hoặc Billboard
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnBaseDestroyed;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
    
    /// <summary>
    /// Nhận sát thương
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        UpdateHealthDisplay();
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            OnBaseDestroyed?.Invoke();
        }
    }
    
    /// <summary>
    /// Hồi máu (nếu cần)
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateHealthDisplay();
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    /// <summary>
    /// Cập nhật hiển thị máu (3D)
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }
    
    /// <summary>
    /// Kiểm tra còn sống không
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    /// <summary>
    /// Lấy HP hiện tại
    /// </summary>
    public float GetCurrentHealth() => currentHealth;
    
    /// <summary>
    /// Lấy HP tối đa
    /// </summary>
    public float GetMaxHealth() => maxHealth;
    
    /// <summary>
    /// Set HP (dùng khi khởi tạo game với độ khó khác nhau)
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
}

