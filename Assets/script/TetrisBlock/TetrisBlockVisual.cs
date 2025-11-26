using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Partial class xử lý visual và rendering cho TetrisBlock
/// </summary>
public partial class TetrisBlock
{
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
                cachedCellRenderers.Add(sr);
            }
            else
            {
                // Thử MeshRenderer (3D)
                MeshRenderer mr = cellObj.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.color = blockColor;
                    cachedCellRenderers.Add(mr);
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
        cachedCellRenderers.Clear();
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
    /// Set alpha cho tất cả block cells
    /// </summary>
    private void SetBlockAlpha(float alpha)
    {
        Color blockColor = GetColorByBlockType();
        blockColor.a = alpha;

        // Set alpha cho cells sử dụng cached renderers
        foreach (Renderer renderer in cachedCellRenderers)
        {
            if (renderer != null)
            {
                // Sử dụng MaterialPropertyBlock để tránh tạo material instance
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(ColorPropertyID, blockColor);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        // Set alpha cho tower nếu có (sử dụng cached renderers)
        if (currentTower != null && cachedTowerRenderers.Count > 0)
        {
            foreach (Renderer renderer in cachedTowerRenderers)
            {
                if (renderer != null)
                {
                    renderer.GetPropertyBlock(propertyBlock);
                    Color towerColor = renderer.sharedMaterial.color;
                    towerColor.a = alpha;
                    propertyBlock.SetColor(ColorPropertyID, towerColor);
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
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
}
