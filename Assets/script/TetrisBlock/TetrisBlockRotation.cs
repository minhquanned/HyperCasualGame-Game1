using UnityEngine;

/// <summary>
/// Partial class xử lý rotation và ghost state cho TetrisBlock
/// </summary>
public partial class TetrisBlock
{
    /// <summary>
    /// Xoay khối 90 độ (chỉ khi chưa đặt)
    /// </summary>
    public void Rotate()
    {
        if (isPlaced) return;

        rotationIndex = (rotationIndex + 1) % 4;
        currentShape = currentShape.Rotate90();
        UpdateVisual(); // Cập nhật visual sau khi xoay
    }

    /// <summary>
    /// Xoay tower khi đã đặt (gọi khi tap)
    /// </summary>
    private void RotateIfPlaced()
    {
        if (!isPlaced || grid == null) return;

        // Nếu đang trong ghost state, không cần giải phóng cells (vì không occupy)
        bool wasInGhostState = isGhostState;

        // Lưu trạng thái hiện tại trước khi xoay
        int oldRotationIndex = rotationIndex;
        BlockShape oldShape = currentShape;

        // Thử xoay
        rotationIndex = (rotationIndex + 1) % 4;
        currentShape = currentShape.Rotate90();

        // Chỉ giải phóng cells nếu không trong ghost state
        if (!wasInGhostState)
        {
            grid.FreeCells(gridIndex, gridPosition, oldShape);
        }

        // Kiểm tra xem có thể đặt ở vị trí mới sau khi xoay không
        if (grid.CanPlaceBlock(gridIndex, gridPosition, currentShape))
        {
            // Có chỗ trống -> xoay bình thường
            UpdateVisual();
            grid.OccupyCells(gridIndex, gridPosition, currentShape);

            // Cập nhật vị trí tower sau khi cells đã xoay
            UpdateTowerPosition();

            // Lưu vị trí hợp lệ
            lastValidPosition = gridPosition;
            lastValidGridIndex = gridIndex;
            lastValidShape = currentShape;
            lastValidRotationIndex = rotationIndex;

            // Thoát ghost state nếu đang ở trong đó
            if (isGhostState)
            {
                SetNormalAlpha();
                isGhostState = false;
            }
        }
        else
        {
            // Không có chỗ -> vào/ở lại ghost state
            UpdateVisual();

            // Cập nhật vị trí tower sau khi cells đã xoay
            UpdateTowerPosition();

            // Lưu vị trí hợp lệ trước đó chỉ lần đầu tiên vào ghost state
            if (!wasInGhostState)
            {
                lastValidPosition = gridPosition;
                lastValidGridIndex = gridIndex;
                lastValidShape = oldShape;
                lastValidRotationIndex = oldRotationIndex;
            }
            // Nếu đã trong ghost state rồi, không cập nhật lastValid* (giữ nguyên vị trí valid cuối cùng)

            EnterGhostState();
        }
    }

    /// <summary>
    /// Chuyển sang ghost state (tăng độ trong suốt)
    /// </summary>
    private void EnterGhostState()
    {
        isGhostState = true;
        SetGhostAlpha();
    }

    /// <summary>
    /// Restore từ ghost state về vị trí hợp lệ trước đó
    /// </summary>
    private void RestoreFromGhostState()
    {
        if (!isGhostState) return;

        // Restore rotation và shape
        rotationIndex = lastValidRotationIndex;
        currentShape = lastValidShape;
        UpdateVisual();

        // Cập nhật vị trí tower sau khi restore cells
        UpdateTowerPosition();

        // Occupy cells lại
        grid.OccupyCells(lastValidGridIndex, lastValidPosition, lastValidShape);

        // Reset alpha
        SetNormalAlpha();
        isGhostState = false;
    }

    /// <summary>
    /// Set alpha cho ghost state (trong suốt hơn)
    /// </summary>
    private void SetGhostAlpha()
    {
        float ghostAlpha = 0.5f;
        SetBlockAlpha(ghostAlpha);
    }

    /// <summary>
    /// Set alpha bình thường
    /// </summary>
    private void SetNormalAlpha()
    {
        SetBlockAlpha(1f);
    }
}
