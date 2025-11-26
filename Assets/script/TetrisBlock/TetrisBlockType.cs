using UnityEngine;

/// <summary>
/// 7 loại khối Tetris truyền thống
/// </summary>
public enum TetrisBlockType
{
    I,  // Khối thẳng
    O,  // Khối vuông
    T,  // Khối T
    S,  // Khối S
    Z,  // Khối Z
    J,  // Khối J
    L   // Khối L
}

/// <summary>
/// Data structure cho hình dạng khối Tetris (các ô chiếm chỗ)
/// </summary>
[System.Serializable]
public class BlockShape
{
    public Vector2Int[] cells; // Vị trí các ô của khối (relative to center)
    
    public BlockShape(Vector2Int[] cells)
    {
        this.cells = cells;
    }
    
    /// <summary>
    /// Xoay khối 90 độ theo chiều kim đồng hồ
    /// </summary>
    public BlockShape Rotate90()
    {
        Vector2Int[] rotated = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            // Xoay 90 độ: (x, y) -> (y, -x)
            rotated[i] = new Vector2Int(cells[i].y, -cells[i].x);
        }
        return new BlockShape(rotated);
    }
}

/// <summary>
/// Static class chứa các hình dạng của 7 khối Tetris
/// </summary>
public static class TetrisBlockShapes
{
    public static BlockShape GetShape(TetrisBlockType type)
    {
        switch (type)
        {
            case TetrisBlockType.I:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0)
                });
            
            case TetrisBlockType.O:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1)
                });
            
            case TetrisBlockType.T:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1)
                });
            
            case TetrisBlockType.S:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 1)
                });
            
            case TetrisBlockType.Z:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1)
                });
            
            case TetrisBlockType.J:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 1)
                });
            
            case TetrisBlockType.L:
                return new BlockShape(new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1)
                });
            
            default:
                return new BlockShape(new Vector2Int[] { new Vector2Int(0, 0) });
        }
    }
}

