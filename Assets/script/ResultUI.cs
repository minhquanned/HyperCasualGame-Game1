using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý màn hình kết thúc game (Win/Lose)
/// </summary>
public class ResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject contentsWin;
    [SerializeField] private GameObject contentsLose;
    [SerializeField] private GameObject btnPanel;

    [Header("Buttons (Optional)")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        // Ẩn tất cả khi khởi tạo
        HideAll();

        // Thêm listeners cho buttons nếu có
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    /// <summary>
    /// Hiển thị màn hình thắng
    /// </summary>
    public void ShowWin()
    {
        HideAll();

        if (overlay != null)
            overlay.SetActive(true);

        if (contentsWin != null)
            contentsWin.SetActive(true);

        if (btnPanel != null)
            btnPanel.SetActive(true);
    }

    /// <summary>
    /// Hiển thị màn hình thua
    /// </summary>
    public void ShowLose()
    {
        HideAll();

        if (overlay != null)
            overlay.SetActive(true);

        if (contentsLose != null)
            contentsLose.SetActive(true);

        if (btnPanel != null)
            btnPanel.SetActive(true);
    }

    /// <summary>
    /// Ẩn tất cả các panel
    /// </summary>
    public void HideAll()
    {
        if (overlay != null)
            overlay.SetActive(false);

        if (contentsWin != null)
            contentsWin.SetActive(false);

        if (contentsLose != null)
            contentsLose.SetActive(false);

        if (btnPanel != null)
            btnPanel.SetActive(false);
    }

    /// <summary>
    /// Restart game
    /// </summary>
    private void OnRestartClicked()
    {
        // Reload scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Về main menu
    /// </summary>
    private void OnMainMenuClicked()
    {
        // Load scene main menu (index 0 hoặc tên scene)
        SceneManager.LoadScene(0);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Test hiển thị màn hình thắng (chỉ trong Editor)
    /// </summary>
    [ContextMenu("Test Show Win")]
    private void TestShowWin()
    {
        ShowWin();
        Debug.Log("✓ Test: Hiển thị màn hình THẮNG");
    }

    /// <summary>
    /// Test hiển thị màn hình thua (chỉ trong Editor)
    /// </summary>
    [ContextMenu("Test Show Lose")]
    private void TestShowLose()
    {
        ShowLose();
        Debug.Log("✓ Test: Hiển thị màn hình THUA");
    }

    /// <summary>
    /// Test ẩn tất cả (chỉ trong Editor)
    /// </summary>
    [ContextMenu("Test Hide All")]
    private void TestHideAll()
    {
        HideAll();
        Debug.Log("✓ Test: Ẩn tất cả màn hình");
    }
#endif
}
