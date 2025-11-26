using UnityEngine;

/// <summary>
/// Partial class xử lý input cho TetrisBlock
/// </summary>
public partial class TetrisBlock
{
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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject
                || hit.collider.transform.parent.gameObject == gameObject)
            {
                pointerDownTime = Time.time;

                // Không restore khi click vào chính tower này
                // Cho phép tap để xoay tiếp hoặc drag

                if (hit.collider.transform.parent.gameObject == gameObject)
                {
                    offsetDrag = hit.collider.transform.position - transform.position;
                }
                originalPosition = transform.position;
                isDragging = true;
                dragPlane = new Plane(Vector3.up, transform.position);

                // Nếu block đã đặt, lưu thông tin và giải phóng cells
                if (isPlaced && grid != null && !isGhostState)
                {
                    wasPlacedBeforeDrag = true;
                    savedGridIndex = gridIndex;
                    savedGridPosition = gridPosition;
                    savedShape = currentShape;

                    // Giải phóng cells để có thể di chuyển
                    grid.FreeCells(gridIndex, gridPosition, currentShape);
                    isPlaced = false; // Tạm thời đánh dấu chưa đặt để có thể di chuyển

                    // Hiển thị delete zone khi bắt đầu drag block đã đặt
                    if (DeleteZone.Instance != null)
                    {
                        DeleteZone.Instance.ShowDeleteZone(this);
                    }
                }
            }
            else
            {
                // Click vào nơi khác khi đang trong ghost state
                if (isGhostState)
                {
                    RestoreFromGhostState();
                }
            }
        }
        else
        {
            // Click vào nơi khác khi đang trong ghost state
            if (isGhostState)
            {
                RestoreFromGhostState();
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
        // Kiểm tra xem có phải tap không (thời gian nhấn ngắn)
        float tapDuration = Time.time - pointerDownTime;
        bool isTap = tapDuration < tapTimeThreshold && !isDragging;

        if (isTap)
        {
            if (isPlaced)
            {
                // Tap vào tower đã đặt -> xoay 90 độ
                RotateIfPlaced();
            }
            else
            {
                // Tap vào block chưa đặt -> xoay block
                Rotate();
            }
            return;
        }

        if (!isDragging) return;

        isDragging = false;

        // Kiểm tra xem có thả vào delete zone không
        Vector2 screenPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        bool inDeleteZone = false;

        if (DeleteZone.Instance != null && wasPlacedBeforeDrag)
        {
            inDeleteZone = DeleteZone.Instance.IsPointerInDeleteZone(screenPos);
            DeleteZone.Instance.HideDeleteZone(); // Ẩn delete zone sau khi thả

            if (inDeleteZone)
            {
                // Xóa tower này
                Debug.Log("Tower deleted!");
                Destroy(gameObject);
                return;
            }
        }

        // Kiểm tra xem có thả vào grid không
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool placedSuccessfully = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
        {
            Vector3 worldPos = hit.point - offsetDrag;
            var (gridIdx, gridPos) = grid.WorldToGridPositionWithIndex(worldPos);

            // Đặt block vào grid (hoặc đặt lại nếu đã đặt trước đó)
            if (PlaceBlock(gridIdx, gridPos, worldPos, wasPlacedBeforeDrag))
            {
                placedSuccessfully = true;
                wasPlacedBeforeDrag = false; // Reset flag
                // Nếu block đã được merge, nó sẽ bị destroy trong MergeWith, nhưng method này đã return rồi nên không sao
                return;
            }
        }

        // Nếu không đặt được và block đã đặt trước đó, restore lại cells cũ
        if (wasPlacedBeforeDrag && grid != null)
        {
            grid.OccupyCells(savedGridIndex, savedGridPosition, savedShape);
            gridIndex = savedGridIndex;
            gridPosition = savedGridPosition;
            transform.position = grid.GridToWorldPosition(savedGridIndex, savedGridPosition);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            isPlaced = true;
            wasPlacedBeforeDrag = false;
        }
        else if (!placedSuccessfully)
        {
            // Nếu không đặt được và không phải block đã đặt, quay về vị trí ban đầu
            transform.position = originalPosition;
        }
    }
}
