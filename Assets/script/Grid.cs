using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quản lý grid 3D, các ô, và mở rộng grid
/// </summary>
public class Grid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int initialWidth = 5;
    [SerializeField] private int initialHeight = 5;
    [SerializeField] private float cellSize = 1f; // Kích thước ô trong world space
    [SerializeField] private float gridSeparation = 10f; // Khoảng cách giữa 2 grid (theo trục Z)
    
    [Header("Visual")]
    [SerializeField] private GameObject cellPrefab; // Prefab có Transform và MeshRenderer/SpriteRenderer
    [SerializeField] private Color availableCellColor = Color.white;
    [SerializeField] private Color unavailableCellColor = Color.gray;
    
    [Header("Non-Grid Cells (Đất liền)")]
    [SerializeField] private GameObject cellNonGridPrefab; // Prefab cho các ô đất liền xung quanh grid
    [SerializeField] private float pathCheckRadius = 0.5f; // Bán kính kiểm tra path
    [SerializeField] private int spawnRadius = 3;  // Bán kính spawn đất liền xung quanh mỗi grid cell
    
    // Enum để phân biệt 2 grid
    public enum GridIndex
    {
        Grid1 = 0,  // Grid thứ nhất
        Grid2 = 1   // Grid thứ hai
    }
    
    // Dictionary lưu trạng thái các ô cho mỗi grid: true = có thể đặt, false = không thể đặt
    private Dictionary<GridIndex, Dictionary<Vector2Int, bool>> cells = new Dictionary<GridIndex, Dictionary<Vector2Int, bool>>();
    private Dictionary<GridIndex, Dictionary<Vector2Int, GameObject>> cellVisuals = new Dictionary<GridIndex, Dictionary<Vector2Int, GameObject>>();
    
    // Dictionary lưu các cellNonGrid (đất liền) - key là world position (Vector3)
    private Dictionary<Vector3, GameObject> cellNonGrids = new Dictionary<Vector3, GameObject>();
    
    // Vị trí center của mỗi grid trong world space
    private Dictionary<GridIndex, Vector3> gridWorldPositions = new Dictionary<GridIndex, Vector3>();
    
    // Reference đến PathManager để kiểm tra path
    private PathManager pathManager;
    
    private void Start()
    {
        // Tìm PathManager
        pathManager = FindFirstObjectByType<PathManager>();
        
        InitializeGrids();
        SpawnNonGridCells();
    }
    
    /// <summary>
    /// Khởi tạo 2 grid: một ở vị trí này, một ở vị trí khác
    /// </summary>
    private void InitializeGrids()
    {
        // Khởi tạo dictionary cho cả 2 grid
        cells[GridIndex.Grid1] = new Dictionary<Vector2Int, bool>();
        cells[GridIndex.Grid2] = new Dictionary<Vector2Int, bool>();
        cellVisuals[GridIndex.Grid1] = new Dictionary<Vector2Int, GameObject>();
        cellVisuals[GridIndex.Grid2] = new Dictionary<Vector2Int, GameObject>();
        
        // Tính toán vị trí world của mỗi grid
        // Grid1: ở phía trước (Z dương)
        gridWorldPositions[GridIndex.Grid1] = transform.position + new Vector3(0, 0, gridSeparation / 2f);
        // Grid2: ở phía sau (Z âm)
        gridWorldPositions[GridIndex.Grid2] = transform.position + new Vector3(0, 0, -gridSeparation / 2f);
        
        int halfWidth = initialWidth / 2;
        int halfHeight = initialHeight / 2;
        
        // Tạo grid 1
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                cells[GridIndex.Grid1][pos] = true;
                CreateCellVisual(GridIndex.Grid1, pos);
            }
        }
        
        // Tạo grid 2
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                cells[GridIndex.Grid2][pos] = true;
                CreateCellVisual(GridIndex.Grid2, pos);
            }
        }
    }
    
    /// <summary>
    /// Đánh dấu GameObject là static để giảm drawcall
    /// </summary>
    private void SetGameObjectStatic(GameObject obj)
    {
        if (obj == null) return;
        
#if UNITY_EDITOR
        // Trong Editor, sử dụng StaticEditorFlags để đánh dấu static
        // Chỉ sử dụng các flags cần thiết cho batching và lighting
        StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
        GameObjectUtility.SetStaticEditorFlags(obj, flags);
#else
        // Trong runtime/build, chỉ set isStatic flag
        obj.isStatic = true;
#endif
    }
    
    /// <summary>
    /// Spawn các cellNonGrid (đất liền) xung quanh các grid cells
    /// </summary>
    private void SpawnNonGridCells()
    {
        if (cellNonGridPrefab == null) return;
        
        HashSet<Vector3> checkedPositions = new HashSet<Vector3>();
        
        // Duyệt qua tất cả các grid cells và spawn đất liền xung quanh
        foreach (var gridPair in cells)
        {
            GridIndex gridIdx = gridPair.Key;
            Vector3 gridCenter = gridWorldPositions[gridIdx];
            
            foreach (Vector2Int cellPos in gridPair.Value.Keys)
            {
                Vector3 cellWorldPos = GridToWorldPosition(gridIdx, cellPos);
                
                // Spawn đất liền xung quanh cell này
                for (int dx = -spawnRadius; dx <= spawnRadius; dx++)
                {
                    for (int dz = -spawnRadius; dz <= spawnRadius; dz++)
                    {
                        if (dx == 0 && dz == 0) continue; // Bỏ qua chính cell đó
                        
                        Vector3 nonGridPos = cellWorldPos + new Vector3(dx * cellSize, 0, dz * cellSize);
                        
                        // Làm tròn vị trí để tránh duplicate
                        nonGridPos = new Vector3(
                            Mathf.Round(nonGridPos.x / cellSize) * cellSize,
                            nonGridPos.y,
                            Mathf.Round(nonGridPos.z / cellSize) * cellSize
                        );
                        
                        // Kiểm tra xem đã spawn ở vị trí này chưa
                        if (checkedPositions.Contains(nonGridPos)) continue;
                        checkedPositions.Add(nonGridPos);
                        
                        // Kiểm tra xem vị trí này có phải là grid cell không
                        bool isGridCell = IsPositionAGridCell(nonGridPos);
                        
                        if (isGridCell) continue; // Đã là grid cell, không spawn đất liền
                        
                        // Kiểm tra xem có nằm trên path không
                        if (IsPositionOnPath(nonGridPos)) continue;
                        
                        // Spawn cellNonGrid
                        GameObject nonGridCell = Instantiate(cellNonGridPrefab, transform);
                        nonGridCell.transform.position = nonGridPos;
                        nonGridCell.transform.localScale = new Vector3(cellSize, cellSize, 0.5f);
                        SetGameObjectStatic(nonGridCell); // Đánh dấu static để giảm drawcall
                        cellNonGrids[nonGridPos] = nonGridCell;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có nằm trên path không
    /// </summary>
    private bool IsPositionOnPath(Vector3 worldPos)
    {
        if (pathManager == null) return false;
        
        List<Vector3> waypoints = pathManager.GetWaypoints();
        if (waypoints.Count < 2) return false;
        
        // Kiểm tra khoảng cách đến các đoạn đường giữa các waypoints
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            
            // Tính khoảng cách từ điểm đến đoạn thẳng
            float distance = DistanceToLineSegment(worldPos, start, end);
            
            if (distance <= pathCheckRadius)
            {
                return true; // Nằm trên path
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Tính khoảng cách từ một điểm đến một đoạn thẳng
    /// </summary>
    private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 line = lineEnd - lineStart;
        float lineLength = line.magnitude;
        
        if (lineLength < 0.001f) // Đoạn thẳng quá ngắn
        {
            return Vector3.Distance(point, lineStart);
        }
        
        Vector3 lineDir = line / lineLength;
        Vector3 pointToStart = point - lineStart;
        
        // Project điểm lên đoạn thẳng
        float projection = Vector3.Dot(pointToStart, lineDir);
        projection = Mathf.Clamp(projection, 0f, lineLength);
        
        // Điểm gần nhất trên đoạn thẳng
        Vector3 closestPoint = lineStart + lineDir * projection;
        
        return Vector3.Distance(point, closestPoint);
    }
    
    /// <summary>
    /// Xác định grid nào dựa trên vị trí world (grid nào gần hơn)
    /// </summary>
    private GridIndex GetGridIndexFromWorldPosition(Vector3 worldPos)
    {
        float distToGrid1 = Vector3.Distance(worldPos, gridWorldPositions[GridIndex.Grid1]);
        float distToGrid2 = Vector3.Distance(worldPos, gridWorldPositions[GridIndex.Grid2]);
        return distToGrid1 < distToGrid2 ? GridIndex.Grid1 : GridIndex.Grid2;
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí world có phải là grid cell không
    /// </summary>
    private bool IsPositionAGridCell(Vector3 worldPos)
    {
        // Kiểm tra cả 2 grid
        foreach (var gridPair in cells)
        {
            GridIndex gridIdx = gridPair.Key;
            Vector3 gridCenter = gridWorldPositions[gridIdx];
            Vector3 localPos = worldPos - gridCenter;
            int x = Mathf.RoundToInt(localPos.x / cellSize);
            int z = Mathf.RoundToInt(localPos.z / cellSize);
            Vector2Int cellPos = new Vector2Int(x, z);
            
            // Kiểm tra xem cell này có trong grid không và có gần với vị trí world không
            if (gridPair.Value.ContainsKey(cellPos))
            {
                Vector3 cellWorldPos = GridToWorldPosition(gridIdx, cellPos);
                float distance = Vector3.Distance(worldPos, cellWorldPos);
                if (distance < cellSize * 0.5f) // Cho phép sai số nhỏ
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Tạo visual cho một ô
    /// </summary>
    private void CreateCellVisual(GridIndex gridIndex, Vector2Int cellPos)
    {
        if (cellPrefab != null)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.transform.position = GridToWorldPosition(gridIndex, cellPos);
            cell.transform.localScale = new Vector3(cellSize, cellSize, 0.1f);
            cellVisuals[gridIndex][cellPos] = cell;
            UpdateCellVisual(gridIndex, cellPos, true);
        }
    }
    
    /// <summary>
    /// Cập nhật màu sắc visual của ô
    /// </summary>
    private void UpdateCellVisual(GridIndex gridIndex, Vector2Int cellPos, bool available)
    {
        if (cellVisuals.ContainsKey(gridIndex) && 
            cellVisuals[gridIndex].ContainsKey(cellPos) && 
            cellVisuals[gridIndex][cellPos] != null)
        {
            GameObject cellObj = cellVisuals[gridIndex][cellPos];
            // Thử SpriteRenderer trước (2D)
            SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = available ? availableCellColor : unavailableCellColor;
                return;
            }
            
            // Nếu không có SpriteRenderer, thử MeshRenderer (3D)
            MeshRenderer mr = cellObj.GetComponent<MeshRenderer>();
            if (mr != null && mr.material != null)
            {
                mr.material.color = available ? availableCellColor : unavailableCellColor;
            }
        }
    }
    
    /// <summary>
    /// Chuyển đổi vị trí grid sang vị trí world
    /// </summary>
    public Vector3 GridToWorldPosition(GridIndex gridIndex, Vector2Int cellPos)
    {
        Vector3 gridCenter = gridWorldPositions[gridIndex];
        return gridCenter + new Vector3(cellPos.x * cellSize, 0, cellPos.y * cellSize);
    }
    
    /// <summary>
    /// Chuyển đổi vị trí grid sang vị trí world (overload - tự động xác định grid gần nhất)
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        // Mặc định dùng grid 1, hoặc có thể tính toán dựa trên context
        return GridToWorldPosition(GridIndex.Grid1, gridPos);
    }
    
    /// <summary>
    /// Chuyển đổi vị trí world sang vị trí grid và grid index
    /// </summary>
    public (GridIndex gridIndex, Vector2Int cellPos) WorldToGridPositionWithIndex(Vector3 worldPos)
    {
        GridIndex gridIndex = GetGridIndexFromWorldPosition(worldPos);
        Vector3 gridCenter = gridWorldPositions[gridIndex];
        Vector3 localPos = worldPos - gridCenter;
        int x = Mathf.RoundToInt(localPos.x / cellSize);
        int y = Mathf.RoundToInt(localPos.z / cellSize); // Dùng Z cho 3D
        return (gridIndex, new Vector2Int(x, y));
    }
    
    /// <summary>
    /// Chuyển đổi vị trí world sang vị trí grid (overload - tự động xác định grid)
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        var (gridIndex, cellPos) = WorldToGridPositionWithIndex(worldPos);
        return cellPos;
    }
    
    /// <summary>
    /// Chuyển đổi vị trí UI local point sang vị trí grid
    /// </summary>
    public Vector2Int UIToGridPosition(Vector2 uiLocalPoint)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("Grid does not have a RectTransform component!");
            return Vector2Int.zero;
        }
        
        // Chuyển đổi UI local point sang world position
        Vector3 worldPos = rectTransform.TransformPoint(uiLocalPoint);
        
        // Sử dụng WorldToGridPosition để chuyển đổi
        return WorldToGridPosition(worldPos);
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có thể đặt khối Tetris không
    /// </summary>
    public bool CanPlaceBlock(GridIndex gridIndex, Vector2Int cellPos, BlockShape shape)
    {
        if (!cells.ContainsKey(gridIndex)) return false;
        
        foreach (Vector2Int cell in shape.cells)
        {
            Vector2Int checkPos = cellPos + cell;
            if (!cells[gridIndex].ContainsKey(checkPos) || !cells[gridIndex][checkPos])
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có thể đặt khối Tetris không (overload - tự động xác định grid)
    /// </summary>
    public bool CanPlaceBlock(Vector3 worldPos, BlockShape shape)
    {
        var (gridIndex, cellPos) = WorldToGridPositionWithIndex(worldPos);
        return CanPlaceBlock(gridIndex, cellPos, shape);
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có thể đặt khối Tetris không (overload - tương thích code cũ)
    /// </summary>
    public bool CanPlaceBlock(Vector2Int gridPos, BlockShape shape)
    {
        // Kiểm tra cả 2 grid
        return CanPlaceBlock(GridIndex.Grid1, gridPos, shape) || CanPlaceBlock(GridIndex.Grid2, gridPos, shape);
    }
    
    /// <summary>
    /// Đánh dấu các ô đã được sử dụng (không thể đặt thêm)
    /// </summary>
    public void OccupyCells(GridIndex gridIndex, Vector2Int cellPos, BlockShape shape)
    {
        if (!cells.ContainsKey(gridIndex)) return;
        
        foreach (Vector2Int cell in shape.cells)
        {
            Vector2Int pos = cellPos + cell;
            if (cells[gridIndex].ContainsKey(pos))
            {
                cells[gridIndex][pos] = false;
                UpdateCellVisual(gridIndex, pos, false);
            }
        }
    }
    
    /// <summary>
    /// Đánh dấu các ô đã được sử dụng (overload - tự động xác định grid)
    /// </summary>
    public void OccupyCells(Vector3 worldPos, BlockShape shape)
    {
        var (gridIndex, cellPos) = WorldToGridPositionWithIndex(worldPos);
        OccupyCells(gridIndex, cellPos, shape);
    }
    
    /// <summary>
    /// Đánh dấu các ô đã được sử dụng (overload - tương thích code cũ)
    /// </summary>
    public void OccupyCells(Vector2Int gridPos, BlockShape shape)
    {
        // Mặc định dùng grid 1
        OccupyCells(GridIndex.Grid1, gridPos, shape);
    }
    
    /// <summary>
    /// Giải phóng các ô (khi khối bị xóa)
    /// </summary>
    public void FreeCells(GridIndex gridIndex, Vector2Int cellPos, BlockShape shape)
    {
        if (!cells.ContainsKey(gridIndex)) return;
        
        foreach (Vector2Int cell in shape.cells)
        {
            Vector2Int pos = cellPos + cell;
            if (cells[gridIndex].ContainsKey(pos))
            {
                cells[gridIndex][pos] = true;
                UpdateCellVisual(gridIndex, pos, true);
            }
        }
    }
    
    /// <summary>
    /// Giải phóng các ô (overload - tự động xác định grid)
    /// </summary>
    public void FreeCells(Vector3 worldPos, BlockShape shape)
    {
        var (gridIndex, cellPos) = WorldToGridPositionWithIndex(worldPos);
        FreeCells(gridIndex, cellPos, shape);
    }
    
    /// <summary>
    /// Giải phóng các ô (overload - tương thích code cũ)
    /// </summary>
    public void FreeCells(Vector2Int gridPos, BlockShape shape)
    {
        // Mặc định dùng grid 1
        FreeCells(GridIndex.Grid1, gridPos, shape);
    }
    
    /// <summary>
    /// Mở rộng grid thêm 1-2 ô cạnh các ô hiện có
    /// </summary>
    public bool ExpandGrid(GridIndex gridIndex, int expansionCount = 1)
    {
        if (!cells.ContainsKey(gridIndex)) return false;
        
        List<Vector2Int> newCells = new List<Vector2Int>();
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
        
        // Tìm các ô có thể mở rộng (cạnh các ô đã có)
        foreach (Vector2Int existingPos in cells[gridIndex].Keys)
        {
            Vector2Int[] neighbors = new Vector2Int[]
            {
                existingPos + Vector2Int.up,
                existingPos + Vector2Int.down,
                existingPos + Vector2Int.left,
                existingPos + Vector2Int.right
            };
            
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!cells[gridIndex].ContainsKey(neighbor) && !checkedPositions.Contains(neighbor))
                {
                    newCells.Add(neighbor);
                    checkedPositions.Add(neighbor);
                }
            }
        }
        
        // Random chọn 1-2 ô để mở rộng
        if (newCells.Count == 0) return false;
        
        int countToAdd = Mathf.Min(expansionCount, newCells.Count);
        for (int i = 0; i < countToAdd; i++)
        {
            int randomIndex = Random.Range(0, newCells.Count);
            Vector2Int newPos = newCells[randomIndex];
            newCells.RemoveAt(randomIndex);
            
            cells[gridIndex][newPos] = true;
            CreateCellVisual(gridIndex, newPos);
        }
        
        return true;
    }
    
    /// <summary>
    /// Mở rộng grid thêm 1-2 ô cạnh các ô hiện có (overload - mở rộng cả 2 grid)
    /// </summary>
    public bool ExpandGrid(int expansionCount = 1)
    {
        bool grid1Expanded = ExpandGrid(GridIndex.Grid1, expansionCount);
        bool grid2Expanded = ExpandGrid(GridIndex.Grid2, expansionCount);
        return grid1Expanded || grid2Expanded;
    }
    
    /// <summary>
    /// Mở rộng grid ở vị trí cụ thể (nếu vị trí đó cạnh các ô hiện có)
    /// </summary>
    public bool ExpandGridAtPosition(GridIndex gridIndex, Vector2Int targetPos, int expansionCount = 1)
    {
        if (!cells.ContainsKey(gridIndex)) return false;
        
        // Kiểm tra xem vị trí này có cạnh các ô hiện có không
        Vector2Int[] directions = new Vector2Int[]
        {
            targetPos + Vector2Int.up,
            targetPos + Vector2Int.down,
            targetPos + Vector2Int.left,
            targetPos + Vector2Int.right
        };
        
        bool isAdjacent = false;
        foreach (Vector2Int neighbor in directions)
        {
            if (cells[gridIndex].ContainsKey(neighbor))
            {
                isAdjacent = true;
                break;
            }
        }
        
        if (!isAdjacent) return false; // Không cạnh grid hiện có
        
        // Nếu vị trí này chưa có, thêm vào
        if (!cells[gridIndex].ContainsKey(targetPos))
        {
            cells[gridIndex][targetPos] = true;
            CreateCellVisual(gridIndex, targetPos);
            
            // Nếu cần mở rộng thêm, tìm các ô xung quanh
            if (expansionCount > 1)
            {
                List<Vector2Int> adjacentNewCells = new List<Vector2Int>();
                foreach (Vector2Int neighbor in directions)
                {
                    if (!cells[gridIndex].ContainsKey(neighbor))
                    {
                        // Kiểm tra xem neighbor có cạnh ô hiện có không
                        Vector2Int[] neighborDirs = new Vector2Int[]
                        {
                            neighbor + Vector2Int.up,
                            neighbor + Vector2Int.down,
                            neighbor + Vector2Int.left,
                            neighbor + Vector2Int.right
                        };
                        
                        foreach (Vector2Int neighborDir in neighborDirs)
                        {
                            if (cells[gridIndex].ContainsKey(neighborDir))
                            {
                                adjacentNewCells.Add(neighbor);
                                break;
                            }
                        }
                    }
                }
                
                // Thêm các ô cạnh (tối đa expansionCount - 1)
                int additionalCount = Mathf.Min(expansionCount - 1, adjacentNewCells.Count);
                for (int i = 0; i < additionalCount; i++)
                {
                    int randomIndex = Random.Range(0, adjacentNewCells.Count);
                    Vector2Int newPos = adjacentNewCells[randomIndex];
                    adjacentNewCells.RemoveAt(randomIndex);
                    
                    cells[gridIndex][newPos] = true;
                    CreateCellVisual(gridIndex, newPos);
                }
            }
            
            return true;
        }
        
        return false; // Vị trí đã tồn tại
    }
    
    /// <summary>
    /// Mở rộng grid ở vị trí world cụ thể (tự động xác định grid)
    /// </summary>
    public bool ExpandGridAtWorldPosition(Vector3 worldPos, int expansionCount = 1)
    {
        
        var (gridIndex, cellPos) = WorldToGridPositionWithIndex(worldPos);
        bool expanded = ExpandGridAtPosition(gridIndex, cellPos, expansionCount);
        
        // Nếu mở rộng thành công, spawn lại đất liền xung quanh vị trí mới
        if (expanded)
        {
            // SpawnNonGridCellsAroundPosition(gridIndex, cellPos);
            // Xóa cellNonGrid ở vị trí này nếu có
            RemoveNonGridCellAtPosition(worldPos);
        }
        
        return expanded;
    }
    
    /// <summary>
    /// Xóa cellNonGrid ở vị trí cụ thể
    /// </summary>
    private void RemoveNonGridCellAtPosition(Vector3 worldPos)
    {
        // Làm tròn vị trí để tìm đúng key
        Vector3 roundedPos = new Vector3(
            Mathf.Round(worldPos.x / cellSize) * cellSize,
            transform.position.y,
            Mathf.Round(worldPos.z / cellSize) * cellSize
        );
        
        if (cellNonGrids.ContainsKey(roundedPos))
        {
            if (cellNonGrids[roundedPos] != null)
            {
                Destroy(cellNonGrids[roundedPos]);
            }
            cellNonGrids.Remove(roundedPos);
        }
    }
    
    /// <summary>
    /// Spawn đất liền xung quanh một vị trí grid mới được mở rộng
    /// </summary>
    private void SpawnNonGridCellsAroundPosition(GridIndex gridIndex, Vector2Int cellPos)
    {
        if (cellNonGridPrefab == null) return;
        
        Vector3 cellWorldPos = GridToWorldPosition(gridIndex, cellPos);
        int spawnRadius = 3;
        
        for (int dx = -spawnRadius; dx <= spawnRadius; dx++)
        {
            for (int dz = -spawnRadius; dz <= spawnRadius; dz++)
            {
                if (dx == 0 && dz == 0) continue; // Bỏ qua chính cell đó
                
                Vector3 nonGridPos = cellWorldPos + new Vector3(dx * cellSize, 0, dz * cellSize);
                
                // Làm tròn vị trí
                nonGridPos = new Vector3(
                    Mathf.Round(nonGridPos.x / cellSize) * cellSize,
                    nonGridPos.y,
                    Mathf.Round(nonGridPos.z / cellSize) * cellSize
                );
                
                // Kiểm tra xem đã có cellNonGrid ở đây chưa
                if (cellNonGrids.ContainsKey(nonGridPos)) continue;
                
                        // Kiểm tra xem vị trí này có phải là grid cell không
                        bool isGridCell = IsPositionAGridCell(nonGridPos);
                
                if (isGridCell) continue; // Đã là grid cell
                
                // Kiểm tra xem có nằm trên path không
                if (IsPositionOnPath(nonGridPos)) continue;
                
                // Spawn cellNonGrid
                GameObject nonGridCell = Instantiate(cellNonGridPrefab, transform);
                nonGridCell.transform.position = nonGridPos;
                nonGridCell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                SetGameObjectStatic(nonGridCell); // Đánh dấu static để giảm drawcall
                cellNonGrids[nonGridPos] = nonGridCell;
            }
        }
    }
    
    /// <summary>
    /// Mở rộng grid ở vị trí cụ thể (overload - tương thích code cũ)
    /// </summary>
    public bool ExpandGridAtPosition(Vector2Int targetPos, int expansionCount = 1)
    {
        // Thử mở rộng cả 2 grid
        bool grid1Expanded = ExpandGridAtPosition(GridIndex.Grid1, targetPos, expansionCount);
        bool grid2Expanded = ExpandGridAtPosition(GridIndex.Grid2, targetPos, expansionCount);
        return grid1Expanded || grid2Expanded;
    }
    
    /// <summary>
    /// Lấy danh sách tất cả các ô có thể đặt
    /// </summary>
    public List<Vector2Int> GetAvailableCells(GridIndex gridIndex)
    {
        List<Vector2Int> available = new List<Vector2Int>();
        if (!cells.ContainsKey(gridIndex)) return available;
        
        foreach (var kvp in cells[gridIndex])
        {
            if (kvp.Value)
            {
                available.Add(kvp.Key);
            }
        }
        return available;
    }
    
    /// <summary>
    /// Lấy danh sách tất cả các ô có thể đặt (overload - từ cả 2 grid)
    /// </summary>
    public List<Vector2Int> GetAvailableCells()
    {
        List<Vector2Int> available = new List<Vector2Int>();
        available.AddRange(GetAvailableCells(GridIndex.Grid1));
        available.AddRange(GetAvailableCells(GridIndex.Grid2));
        return available;
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có trong grid không
    /// </summary>
    public bool IsValidPosition(GridIndex gridIndex, Vector2Int cellPos)
    {
        return cells.ContainsKey(gridIndex) && cells[gridIndex].ContainsKey(cellPos);
    }
    
    /// <summary>
    /// Kiểm tra xem một vị trí có trong grid không (overload - tương thích code cũ)
    /// </summary>
    public bool IsValidPosition(Vector2Int gridPos)
    {
        return IsValidPosition(GridIndex.Grid1, gridPos) || IsValidPosition(GridIndex.Grid2, gridPos);
    }
    
    /// <summary>
    /// Lấy kích thước cell (pixels)
    /// </summary>
    public float GetCellSize()
    {
        return cellSize;
    }
}
