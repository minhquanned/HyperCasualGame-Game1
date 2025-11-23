using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Item người chơi nhận được mỗi turn (UI Canvas) - có thể kéo thả
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    [SerializeField] private ItemType itemType;
    
    [Header("Tetris Block (nếu là TetrisBlock)")]
    [SerializeField] private TetrisBlockType blockType;
    
    [Header("Visual")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    
    [Header("Spawn Settings")]
    [SerializeField] private GameObject tetrisBlockPrefab; // Prefab của TetrisBlock
    [SerializeField] private Transform blockSpawnParent; // Parent để spawn block (thường là Canvas hoặc một container)
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private bool isDragging = false;
    private bool isUsed = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }
    
    private void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
    }
    
    /// <summary>
    /// Khởi tạo item
    /// </summary>
    public void Initialize(ItemType type, TetrisBlockType? block = null)
    {
        itemType = type;
        if (block.HasValue)
        {
            blockType = block.Value;
        }
        
        UpdateVisual();
    }
    
    /// <summary>
    /// Drag handlers - kéo thả item
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isUsed) return;
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isUsed) return;
        
        // Di chuyển item theo chuột
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isUsed) return;
        
        isDragging = false;
        
        // Kiểm tra xem có thả vào grid không
        Grid grid = FindFirstObjectByType<Grid>();
        if (grid != null)
        {
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            Vector2 localPoint;
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect, 
                eventData.position, 
                canvas.worldCamera, 
                out localPoint))
            {
                // Chuyển đổi sang grid position
                Vector2Int gridPos = grid.UIToGridPosition(localPoint);
                
                // Sử dụng item tùy theo loại
                if (itemType == ItemType.TetrisBlock)
                {
                    PlaceTetrisBlock(gridPos);
                }
                else if (itemType == ItemType.GridExpansion)
                {
                    PlaceGridExpansion(gridPos);
                }
                
                // Xóa item sau khi dùng
                isUsed = true;
                Destroy(gameObject);
                return;
            }
        }
        
        // Nếu không thả vào grid, quay về vị trí ban đầu
        rectTransform.anchoredPosition = originalPosition;
    }
    
    /// <summary>
    /// Đặt TetrisBlock vào grid
    /// </summary>
    private void PlaceTetrisBlock(Vector2Int gridPos)
    {
        if (tetrisBlockPrefab == null) return;
        
        Grid grid = FindFirstObjectByType<Grid>();
        if (grid == null) return;
        
        // Tìm parent để spawn
        Transform parent = blockSpawnParent;
        if (parent == null)
        {
            parent = canvas != null ? canvas.transform : transform.parent;
        }
        
        // Spawn block
        GameObject blockObj = Instantiate(tetrisBlockPrefab, parent);
        TetrisBlock block = blockObj.GetComponent<TetrisBlock>();
        
        if (block != null)
        {
            // Khởi tạo block
            block.Initialize(blockType, 1); // Level 1 ban đầu
            
            // Đặt block vào grid
            block.PlaceBlock(gridPos);
        }
    }
    
    /// <summary>
    /// Đặt GridExpansion vào cạnh grid
    /// </summary>
    private void PlaceGridExpansion(Vector2Int gridPos)
    {
        Grid grid = FindFirstObjectByType<Grid>();
        if (grid == null) return;
        
        // Mở rộng grid ở vị trí này (nếu có thể)
        grid.ExpandGridAtPosition(gridPos, 1);
    }
    
    /// <summary>
    /// Cập nhật visual
    /// </summary>
    private void UpdateVisual()
    {
        if (itemNameText != null)
        {
            switch (itemType)
            {
                case ItemType.TetrisBlock:
                    itemNameText.text = blockType.ToString();
                    break;
                case ItemType.GridExpansion:
                    itemNameText.text = "Expand";
                    break;
            }
        }
    }
    
    // Getters
    public ItemType GetItemType() => itemType;
    public TetrisBlockType GetBlockType() => blockType;
    public bool IsUsed() => isUsed;
}

