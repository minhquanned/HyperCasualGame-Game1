using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Partial class xử lý tower management cho TetrisBlock
/// </summary>
public partial class TetrisBlock
{
    private void SpawnTower()
    {
        if (towerPrefab == null || blockCells.Count == 0) return;

        // Xóa tower cũ nếu có
        if (currentTower != null)
        {
            Destroy(currentTower.gameObject);
            currentTower = null;
        }

        // Xóa cache tower renderers cũ
        cachedTowerRenderers.Clear();

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
        GameObject towerObj = Instantiate(towerPrefab, towerPosition, Quaternion.identity, transform);
        currentTower = towerObj.GetComponent<Tower>();
        if (currentTower != null)
        {
            currentTower.SetLevel(level);

            // Cache tower renderers
            Renderer[] renderers = currentTower.GetComponentsInChildren<Renderer>();
            cachedTowerRenderers.AddRange(renderers);
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

        // Xóa cache
        cachedTowerRenderers.Clear();

        // Spawn lại tower với level mới tại trung điểm mới
        SpawnTower();
    }

    /// <summary>
    /// Cập nhật vị trí tower dựa trên trung điểm các cells (không spawn lại)
    /// </summary>
    private void UpdateTowerPosition()
    {
        if (currentTower == null || blockCells.Count == 0) return;

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
        currentTower.transform.position = towerPosition;
    }
}
