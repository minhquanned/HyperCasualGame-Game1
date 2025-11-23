using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Khối Tetris có thể xoay, di chuyển, và merge (3D)
/// </summary>
public class TetrisBlock : MonoBehaviour
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
    
    [Header("Input")]
    [SerializeField] private float tapTimeThreshold = 0.2f; // Thời gian để phân biệt tap vs hold
    
    private BlockShape currentShape;
    private int rotationIndex = 0; // 0, 1, 2, 3 (0°, 90°, 180°, 270°)
    private Vector2Int gridPosition;
    private Grid.GridIndex gridIndex = Grid.GridIndex.Grid1; // Grid nào block này đang ở
    private Grid grid;
    private List<Transform> blockCells = new List<Transform>();
    private Tower currentTower; // Reference đến tower đã spawn
    private bool isPlaced = false;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 offsetDrag;
    private Plane dragPlane; // Plane để drag trong 3D
    
    // Merge system
    private List<TetrisBlock> adjacentBlocks = new List<TetrisBlock>();
    
    // Visual system - lưu các cell đã spawn
    private List<GameObject> spawnedBlockCells = new List<GameObject>();
    
    private void Awake()
    {
        grid = FindFirstObjectByType<Grid>();
        currentShape = TetrisBlockShapes.GetShape(blockType);
        
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
        if (isPlaced) return;
        
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
    /// Xử lý input cho 3D (mouse/touch)
    /// </summary>
    private void HandleInput()
    {
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            OnPointerDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnPointerUp();
        }
        
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnPointerDown();
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                OnDrag();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                OnPointerUp();
            }
        }
    }
    
    private void OnPointerDown()
    {
        if (isPlaced) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject == gameObject
                || hit.collider.transform.parent.gameObject == gameObject)
            {
                if(hit.collider.transform.parent.gameObject == gameObject)
                {
                    offsetDrag = hit.collider.transform.position - transform.position;
                }
                originalPosition = transform.position;
                isDragging = true;
                dragPlane = new Plane(Vector3.up, transform.position);
            }
        }
    }
    
    private void OnDrag()
    {
        if (isPlaced || !isDragging) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        
        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            transform.position = hitPoint - offsetDrag;
        }
    }
    
    private void OnPointerUp()
    {
        if (isPlaced || !isDragging) return;
        
        isDragging = false;
        
        // Kiểm tra xem có thả vào grid không
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
        {
            Vector3 worldPos = hit.point - offsetDrag;
            var (gridIdx, gridPos) = grid.WorldToGridPositionWithIndex(worldPos);
            
            // Đặt block vào grid
            if (PlaceBlock(gridIdx, gridPos, worldPos))
            {
                return;
            }
        }
        
        // Nếu không đặt được, quay về vị trí ban đầu
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// Khởi tạo khối với type và level
    /// </summary>
    public void Initialize(TetrisBlockType type, int initialLevel = 1)
    {
        blockType = type;
        level = initialLevel;
        currentShape = TetrisBlockShapes.GetShape(blockType);
        UpdateVisual();
        UpdateLevelDisplay();
    }
    
    /// <summary>
    /// Xoay khối 90 độ (trong 3D, xoay quanh trục Y)
    /// </summary>
    public void Rotate()
    {
        if (isPlaced) return;
        
        rotationIndex = (rotationIndex + 1) % 4;
        currentShape = currentShape.Rotate90();
        transform.Rotate(0, 90, 0); // Xoay quanh trục Y trong 3D
        UpdateVisual(); // Cập nhật visual sau khi xoay
    }
    
    /// <summary>
    /// Đặt khối vào grid
    /// </summary>
    public bool PlaceBlock(Grid.GridIndex gridIdx, Vector2Int gridPos, Vector3 worldPos)
    {
        if (grid == null) return false;
        
        // Kiểm tra xem có thể đặt hoặc merge không
        TetrisBlock mergeTarget = FindBlockToMerge(gridIdx, gridPos);
        
        if (mergeTarget != null)
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
            grid.OccupyCells(gridIdx, gridPos, currentShape);
            isPlaced = true;
            originalPosition = transform.position;
            
            // Thêm Tower component để bắn
            SpawnTower();
            
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
    
    /// <summary>
    /// Tìm khối có thể merge ở vị trí này (cùng type và level)
    /// </summary>
    private TetrisBlock FindBlockToMerge(Grid.GridIndex gridIdx, Vector2Int gridPos)
    {
        // Kiểm tra các ô mà khối này sẽ chiếm
        foreach (Vector2Int cell in currentShape.cells)
        {
            Vector2Int checkPos = gridPos + cell;
            
            // Tìm khối ở vị trí này (phải cùng grid)
            TetrisBlock[] allBlocks = FindObjectsByType<TetrisBlock>(FindObjectsSortMode.None);
            foreach (TetrisBlock block in allBlocks)
            {
                if (block != null && block != this && 
                    block.blockType == this.blockType && 
                    block.level == this.level &&
                    block.isPlaced &&
                    block.gridIndex == gridIdx) // Phải cùng grid
                {
                    // Kiểm tra xem khối này có chiếm vị trí checkPos không
                    foreach (Vector2Int blockCell in block.currentShape.cells)
                    {
                        Vector2Int blockCellPos = block.gridPosition + blockCell;
                        if (blockCellPos == checkPos)
                        {
                            return block; // Tìm thấy khối để merge
                        }
                    }
                }
            }
        }
        
        return null;
    }

    private void SpawnTower()
    {
        if (towerPrefab == null || blockCells.Count == 0) return;
        
        // Xóa tower cũ nếu có
        if (currentTower != null)
        {
            Destroy(currentTower.gameObject);
            currentTower = null;
        }
        
        // Tính toán trung điểm của các cell
        Vector3 sum = Vector3.zero;
        foreach (Transform cell in blockCells)
        {
            if (cell != null)
            {
                sum += cell.position;
            }
        }

        Vector3 towerPosition = sum / blockCells.Count;
        
        // Spawn tower mới
        GameObject towerObj = Instantiate(towerPrefab, towerPosition, Quaternion.identity);
        currentTower = towerObj.GetComponent<Tower>();
        if (currentTower != null)
        {
            currentTower.SetLevel(level);
        }
    }
    
    /// <summary>
    /// Cập nhật tower sau khi merge (level mới và vị trí mới)
    /// </summary>
    private void UpdateTowerAfterMerge()
    {
        if (blockCells.Count == 0) return;
        
        // Xóa tower cũ nếu có
        if (currentTower != null)
        {
            Destroy(currentTower.gameObject);
            currentTower = null;
        }
        
        // Spawn lại tower với level mới tại trung điểm mới
        SpawnTower();
    }
    
    /// <summary>
    /// Kiểm tra và merge với các khối cạnh nhau
    /// </summary>
    private void CheckAndMerge()
    {
        // Tìm các khối cùng type và level ở cạnh
        List<TetrisBlock> mergeCandidates = FindAdjacentSameBlocks();
        
        if (mergeCandidates.Count > 0)
        {
            // Merge với khối đầu tiên tìm thấy
            TetrisBlock targetBlock = mergeCandidates[0];
            MergeWith(targetBlock);
        }
    }
    
    /// <summary>
    /// Tìm các khối cùng type và level ở cạnh
    /// </summary>
    private List<TetrisBlock> FindAdjacentSameBlocks()
    {
        List<TetrisBlock> candidates = new List<TetrisBlock>();
        
        // Kiểm tra 4 hướng
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = gridPosition + dir;
            
            // Tìm khối ở vị trí này bằng cách duyệt tất cả TetrisBlock (phải cùng grid)
            TetrisBlock[] allBlocks = FindObjectsByType<TetrisBlock>(FindObjectsSortMode.None);
            foreach (TetrisBlock block in allBlocks)
            {
                if (block != null && block != this && 
                    block.blockType == this.blockType && 
                    block.level == this.level &&
                    block.isPlaced &&
                    block.gridIndex == this.gridIndex && // Phải cùng grid
                    block.gridPosition == checkPos)
                {
                    candidates.Add(block);
                    break;
                }
            }
        }
        
        return candidates;
    }
    
    /// <summary>
    /// Merge với một khối khác (được gọi từ khối khác hoặc từ chính nó)
    /// </summary>
    public void MergeWith(TetrisBlock otherBlock)
    {
        // Đảm bảo khối này là khối được giữ lại (khối đã đặt trên grid)
        // Khối otherBlock sẽ bị xóa
        
        // Tăng level
        int newLevel = level + 1;
        
        // Lưu lại thông tin
        Grid.GridIndex thisGridIdx = gridIndex;
        Vector2Int thisPos = gridPosition;
        BlockShape thisShape = currentShape;
        Grid.GridIndex otherGridIdx = otherBlock.gridIndex;
        Vector2Int otherPos = otherBlock.gridPosition;
        BlockShape otherShape = otherBlock.currentShape;
        
        // Xóa khối cũ (sử dụng đúng grid index)
        grid.FreeCells(thisGridIdx, thisPos, thisShape);
        grid.FreeCells(otherGridIdx, otherPos, otherShape);
        
        // Khởi tạo lại khối này với level mới
        level = newLevel;
        UpdateLevelDisplay();
        
        // Không cần cập nhật visual vì màu giữ nguyên theo type, chỉ levelText thay đổi
        
        // Đặt lại vào grid (giữ nguyên vị trí hiện tại và grid index)
        grid.OccupyCells(thisGridIdx, thisPos, thisShape);
        
        // Xóa khối kia (sẽ tự động xóa tower của nó khi destroy)
        Destroy(otherBlock.gameObject);
        
        // Cập nhật tower với level mới và vị trí mới
        UpdateTowerAfterMerge();
        
        // Kiểm tra merge tiếp (nếu có khối khác cùng level mới)
        CheckAndMerge();
    }
    
    /// <summary>
    /// Xử lý click để xoay (double click hoặc tap)
    /// </summary>
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;
    
    private void OnMouseDown()
    {
        if (isPlaced) return;
        
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick < doubleClickTime)
        {
            // Double click = xoay
            Rotate();
        }
        lastClickTime = Time.time;
    }
    
    /// <summary>
    /// Cập nhật visual của khối - spawn cells theo shape (3D)
    /// </summary>
    private void UpdateVisual()
    {
        // Xóa các cell cũ
        ClearBlockCells();
        
        if (blockCellPrefab == null || grid == null) return;
        
        // Lấy cellSize từ grid
        float cellSize = grid != null ? grid.GetCellSize() : 1f;
        
        // Spawn cell cho mỗi cell trong shape
        Transform container = cellContainer != null ? cellContainer : transform;
        
        foreach (Vector2Int cell in currentShape.cells)
        {
            GameObject cellObj = Instantiate(blockCellPrefab, container);
            blockCells.Add(cellObj.transform);

            // Đặt vị trí theo cell position (tính từ center)
            cellObj.transform.localPosition = new Vector3(cell.x * cellSize, 0, cell.y * cellSize);
            cellObj.transform.localScale = Vector3.one * cellSize;
            
            // Áp dụng màu sắc theo block type
            Color blockColor = GetColorByBlockType();
            
            // Thử SpriteRenderer trước (2D)
            SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = blockColor;
            }
            else
            {
                // Thử MeshRenderer (3D)
                MeshRenderer mr = cellObj.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.color = blockColor;
                }
            }
            
            spawnedBlockCells.Add(cellObj);
        }
    }
    
    /// <summary>
    /// Xóa tất cả các cell block đã spawn
    /// </summary>
    private void ClearBlockCells()
    {
        blockCells.Clear();
        foreach (GameObject cell in spawnedBlockCells)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }
        spawnedBlockCells.Clear();
    }
    
    /// <summary>
    /// Lấy màu sắc theo block type
    /// </summary>
    private Color GetColorByBlockType()
    {
        switch (blockType)
        {
            case TetrisBlockType.I:
                return colorI;
            case TetrisBlockType.O:
                return colorO;
            case TetrisBlockType.T:
                return colorT;
            case TetrisBlockType.S:
                return colorS;
            case TetrisBlockType.Z:
                return colorZ;
            case TetrisBlockType.J:
                return colorJ;
            case TetrisBlockType.L:
                return colorL;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Cập nhật hiển thị level
    /// </summary>
    private void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            levelText.text = "L" + level;
        }
    }
    
    // Getters
    public TetrisBlockType GetBlockType() => blockType;
    public int GetLevel() => level;
    public BlockShape GetShape() => currentShape;
    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsPlaced() => isPlaced;
}

