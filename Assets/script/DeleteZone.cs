using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vùng UI để xóa tower khi kéo vào
/// Hiển thị ở góc dưới màn hình khi đang drag tower
/// </summary>
public class DeleteZone : MonoBehaviour
{
    public static DeleteZone Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject deleteZonePanel; // Panel chứa UI delete zone
    [SerializeField] private Image deleteZoneImage; // Image của delete zone
    [SerializeField] private RectTransform deleteZoneRect; // RectTransform để check bounds

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(1f, 0.3f, 0.3f, 0.5f); // Màu đỏ nhạt
    [SerializeField] private Color hoverColor = new Color(1f, 0f, 0f, 0.8f); // Màu đỏ đậm khi hover
    [SerializeField] private float scaleOnHover = 1.1f; // Scale lên khi hover

    private bool isHovering = false;
    private Vector3 originalScale;
    private TetrisBlock currentDraggingBlock = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (deleteZonePanel != null)
        {
            originalScale = deleteZonePanel.transform.localScale;
            HideDeleteZone(); // Ẩn ban đầu
        }
    }

    /// <summary>
    /// Hiển thị delete zone khi bắt đầu drag
    /// </summary>
    public void ShowDeleteZone(TetrisBlock draggingBlock)
    {
        if (deleteZonePanel != null)
        {
            deleteZonePanel.SetActive(true);
            currentDraggingBlock = draggingBlock;
            isHovering = false;
            UpdateVisual();
        }
    }

    /// <summary>
    /// Ẩn delete zone khi kết thúc drag
    /// </summary>
    public void HideDeleteZone()
    {
        if (deleteZonePanel != null)
        {
            deleteZonePanel.SetActive(false);
            currentDraggingBlock = null;
            isHovering = false;
        }
    }

    /// <summary>
    /// Kiểm tra xem vị trí screen có nằm trong delete zone không
    /// </summary>
    public bool IsPointerInDeleteZone(Vector2 screenPosition)
    {
        if (deleteZoneRect == null) return false;

        // Chuyển screen position sang local position trong RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            deleteZoneRect,
            screenPosition,
            null, // Null vì là Screen Space Overlay
            out Vector2 localPoint
        );

        // Kiểm tra xem point có nằm trong rect không
        bool wasHovering = isHovering;
        isHovering = deleteZoneRect.rect.Contains(localPoint);

        // Cập nhật visual nếu trạng thái hover thay đổi
        if (wasHovering != isHovering)
        {
            UpdateVisual();
        }

        return isHovering;
    }

    /// <summary>
    /// Cập nhật visual (màu sắc, scale) dựa vào trạng thái hover
    /// </summary>
    private void UpdateVisual()
    {
        if (deleteZoneImage == null || deleteZonePanel == null) return;

        if (isHovering)
        {
            deleteZoneImage.color = hoverColor;
            deleteZonePanel.transform.localScale = originalScale * scaleOnHover;
        }
        else
        {
            deleteZoneImage.color = normalColor;
            deleteZonePanel.transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Update để kiểm tra vị trí mouse/touch khi đang drag
    /// </summary>
    private void Update()
    {
        if (currentDraggingBlock != null && deleteZonePanel.activeSelf)
        {
            Vector2 screenPos;

            // Kiểm tra input từ mouse hoặc touch
            if (Input.touchCount > 0)
            {
                screenPos = Input.GetTouch(0).position;
            }
            else
            {
                screenPos = Input.mousePosition;
            }

            IsPointerInDeleteZone(screenPos);
        }
    }

    /// <summary>
    /// Lấy trạng thái hover hiện tại
    /// </summary>
    public bool IsHovering()
    {
        return isHovering;
    }
}
