using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Partial class xử lý merge logic cho TetrisBlock
/// </summary>
public partial class TetrisBlock
{
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
}
