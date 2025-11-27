using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Khối Tetris có thể xoay, di chuyển, và merge (3D)
/// Main class - Core logic và placement
/// </summary>
public partial class TetrisBlock : MonoBehaviour
{
    [Header("Block Properties")]
    [SerializeField] private TetrisBlockType blockType;
    [SerializeField] private int level = 1;

    [Header("Tower")]
    [SerializeField] private GameObject towerPrefab;

    [Header("Visual")]
    [SerializeField] private TMPro.TextMeshProUGUI levelText; // Hiển thị level (3D)
    [SerializeField] private GameObject blockCellPrefab; // Prefab cho mỗi cell của block (MeshRenderer hoặc SpriteRenderer)
    [SerializeField] private Transform cellContainer; // Container cho các cell block

    [Header("Input")]
    [SerializeField] private Camera mainCamera; // Camera để raycast
    [SerializeField] private LayerMask gridLayerMask = -1; // Layer mask cho grid

    [Header("Color Settings - Màu cho mỗi block type")]
    [SerializeField] private Color colorI = new Color(0f, 1f, 1f); // Cyan
    [SerializeField] private Color colorO = new Color(1f, 1f, 0f); // Yellow
    [SerializeField] private Color colorT = new Color(1f, 0f, 1f); // Magenta
    [SerializeField] private Color colorS = new Color(0f, 1f, 0f); // Green
    [SerializeField] private Color colorZ = new Color(1f, 0f, 0f); // Red
    [SerializeField] private Color colorJ = new Color(0f, 0f, 1f); // Blue
    [SerializeField] private Color colorL = new Color(1f, 0.5f, 0f); // Orange

    [Header("Input Settings")]
    [SerializeField] private float tapTimeThreshold = 0.2f; // Thời gian để phân biệt tap vs hold

    // Core state
    private BlockShape currentShape;
    private int rotationIndex = 0; // 0, 1, 2, 3 (0°, 90°, 180°, 270°)
    private Vector2Int gridPosition;
    private Grid.GridIndex gridIndex = Grid.GridIndex.Grid1; // Grid nào block này đang ở
    private Grid grid;
    private List<Transform> blockCells = new List<Transform>();
    private Tower currentTower; // Reference đến tower đã spawn
    private bool isPlaced = false;

    // Input state
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 offsetDrag;
    private Plane dragPlane; // Plane để drag trong 3D
    private float pointerDownTime = 0f;

    // Rotation & ghost state
    private bool isGhostState = false; // Trạng thái tạm thời khi không có chỗ xoay
    private Vector2Int lastValidPosition;
    private Grid.GridIndex lastValidGridIndex;
    private BlockShape lastValidShape;
    private int lastValidRotationIndex;

    // Drag state
    private Grid.GridIndex savedGridIndex;
    private Vector2Int savedGridPosition;
    private BlockShape savedShape;
    private bool wasPlacedBeforeDrag = false;

    // Visual & rendering
    private List<GameObject> spawnedBlockCells = new List<GameObject>();
    private List<Renderer> cachedCellRenderers = new List<Renderer>();
    private List<Renderer> cachedTowerRenderers = new List<Renderer>();
    private MaterialPropertyBlock propertyBlock;
    private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        grid = FindFirstObjectByType<Grid>();
        currentShape = TetrisBlockShapes.GetShape(blockType);
        propertyBlock = new MaterialPropertyBlock();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        dragPlane = new Plane(Vector3.up, transform.position);

        UpdateVisual();
    }

    private void Start()
    {
        UpdateLevelDisplay();
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Cho phép input ngay cả khi đã đặt để có thể kéo thả lại
        HandleInput();
    }

    private void OnDestroy()
    {
        // Xóa tower nếu có
        if (currentTower != null)
        {
            Destroy(currentTower.gameObject);
            currentTower = null;
        }

        // Xóa tất cả cells khi object bị destroy
        ClearBlockCells();
    }

    /// <summary>
    /// Khởi tạo khối với type và level
    /// </summary>
    public void Initialize(TetrisBlockType type, int initialLevel = 1)
    {
        blockType = type;
        level = Mathf.Clamp(initialLevel, 1, 9);
        currentShape = TetrisBlockShapes.GetShape(blockType);
        UpdateVisual();
        UpdateLevelDisplay();
    }

    /// <summary>
    /// Đặt khối vào grid
    /// </summary>
    public bool PlaceBlock(Grid.GridIndex gridIdx, Vector2Int gridPos, Vector3 worldPos, bool isReplacing = false)
    {
        if (grid == null) return false;

        // Nếu đang đặt lại và vị trí mới giống vị trí cũ, chỉ cần restore lại
        if (isReplacing && savedGridIndex == gridIdx && savedGridPosition == gridPos)
        {
            grid.OccupyCells(gridIdx, gridPos, currentShape);
            gridIndex = gridIdx;
            gridPosition = gridPos;
            transform.position = grid.GridToWorldPosition(gridIdx, gridPos);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            isPlaced = true;
            originalPosition = transform.position;
            return true;
        }

        // Kiểm tra xem có thể đặt hoặc merge không
        TetrisBlock mergeTarget = FindBlockToMerge(gridIdx, gridPos);

        if (mergeTarget != null && mergeTarget != this) // Không merge với chính nó
        {
            // Merge với khối tìm thấy
            mergeTarget.MergeWith(this);
            return true;
        }
        else if (grid.CanPlaceBlock(gridIdx, gridPos, currentShape))
        {
            // Đặt khối bình thường
            gridIndex = gridIdx;
            gridPosition = gridPos;
            transform.position = grid.GridToWorldPosition(gridIdx, gridPos);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            grid.OccupyCells(gridIdx, gridPos, currentShape);
            isPlaced = true;
            originalPosition = transform.position;

            // Lưu vị trí hợp lệ cho rotate system
            lastValidPosition = gridPos;
            lastValidGridIndex = gridIdx;
            lastValidShape = currentShape;
            lastValidRotationIndex = rotationIndex;

            // Thêm Tower component để bắn (chỉ spawn nếu chưa có)
            if (currentTower == null)
            {
                SpawnTower();
            }
            else
            {
                // Cập nhật vị trí tower nếu đã có
                UpdateTowerAfterMerge();
            }

            // Kiểm tra merge với các khối xung quanh
            CheckAndMerge();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Đặt khối vào grid (overload để tương thích với code cũ)
    /// </summary>
    public bool PlaceBlock(Vector2Int gridPos)
    {
        // Mặc định dùng grid 1 và tính world position
        Vector3 worldPos = grid != null ? grid.GridToWorldPosition(Grid.GridIndex.Grid1, gridPos) : transform.position;
        return PlaceBlock(Grid.GridIndex.Grid1, gridPos, worldPos);
    }

    // Getters & Setters
    public TetrisBlockType GetBlockType() => blockType;
    public int GetLevel() => level;
    
    /// <summary>
    /// Set level với clamp tự động (1-9)
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = Mathf.Clamp(newLevel, 1, 9);
        UpdateLevelDisplay();
        if (currentTower != null)
        {
            currentTower.SetLevel(level);
        }
    }
    
    public BlockShape GetShape() => currentShape;
    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsPlaced() => isPlaced;
}
