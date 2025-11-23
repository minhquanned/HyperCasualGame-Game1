using UnityEngine;

/// <summary>
/// Block mở rộng grid - có thể kéo thả vào cạnh grid để mở rộng (3D)
/// </summary>
public class BlockExpandGrid : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private TMPro.TextMeshPro expandText; // 3D text
    
    [Header("Input")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayerMask = -1;
    [SerializeField] private LayerMask nonGridLayerMask = -1; // Layer mask cho cellNonGrid
    
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool isUsed = false;
    private Plane dragPlane;
    
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        dragPlane = new Plane(Vector3.up, transform.position);
    }
    
    private void Start()
    {
        originalPosition = transform.position;
        if (expandText != null)
        {
            expandText.text = "Expand";
        }
    }
    
    private void Update()
    {
        if (isUsed) return;
        
        HandleInput();
    }
    
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
    }
    
    private void OnPointerDown()
    {
        if (isUsed) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            originalPosition = transform.position;
            dragPlane = new Plane(Vector3.up, transform.position);
        }
    }
    
    private void OnDrag()
    {
        if (isUsed || !isDragging) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        
        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            transform.position = hitPoint;
        }
    }
    
    private void OnPointerUp()
    {
        if (isUsed || !isDragging) return;
        
        isDragging = false;
        
        // Kiểm tra xem có thả vào grid hoặc cellNonGrid không
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        Grid grid = FindFirstObjectByType<Grid>();
        if (grid != null)
        {
            // Thử raycast vào grid layer hoặc nonGrid layer
            LayerMask combinedMask = gridLayerMask | nonGridLayerMask;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, combinedMask))
            {
                // Sử dụng method mới để tự động xác định grid dựa trên vị trí world
                // Method này sẽ tự động xóa cellNonGrid nếu có
                if (grid.ExpandGridAtWorldPosition(hit.point, 1))
                {
                    // Xóa block sau khi dùng
                    isUsed = true;
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        // Nếu không thả vào grid hoặc không mở rộng được, quay về vị trí ban đầu
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// Kiểm tra đã được sử dụng chưa
    /// </summary>
    public bool IsUsed() => isUsed;
}

