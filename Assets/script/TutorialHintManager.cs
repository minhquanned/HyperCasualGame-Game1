using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý hệ thống hướng dẫn chơi game - hiển thị hint animation
/// Đảm bảo chỉ hiện 1 hint tại một thời điểm và mỗi hint chỉ hiện 1 lần mỗi session
/// Animation sẽ lặp lại cho đến khi người chơi thực hiện thao tác
/// </summary>
public class TutorialHintManager : MonoBehaviour
{
    public static TutorialHintManager Instance { get; private set; }

    [Header("Hint Prefab")]
    [SerializeField] private GameObject hintDotPrefab; // Prefab chấm tròn UI với animation

    [Header("Settings")]
    [SerializeField] private float hintDuration = 2f; // Thời gian hiển thị hint

    // Event khi hint hoàn thành
    public event Action OnHintCompleted;

    // Enum các loại hint
    public enum HintType
    {
        TetrisBlockPlacement,    // Hướng dẫn đặt tetris block
        BlockExpandGridPlacement // Hướng dẫn đặt block expand grid
    }

    // Tracking hints đã hiển thị
    private HashSet<HintType> shownHints = new HashSet<HintType>();

    // Hint hiện tại đang hiển thị
    private GameObject currentHintObject = null;
    private TutorialHintAnimation currentHintAnimation = null;
    private bool isShowingHint = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Kiểm tra input để dừng hint animation
        if (isShowingHint && currentHintAnimation != null)
        {
            if (DetectPlayerInput())
            {
                StopCurrentHint();
            }
        }
    }

    /// <summary>
    /// Phát hiện bất kỳ input nào từ người chơi
    /// </summary>
    private bool DetectPlayerInput()
    {
        // Kiểm tra mouse input
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            return true;
        }

        // Kiểm tra touch input (mobile)
        if (Input.touchCount > 0)
        {
            return true;
        }

        // Kiểm tra keyboard input (bất kỳ phím nào)
        if (Input.anyKeyDown)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dừng hint hiện tại
    /// </summary>
    private void StopCurrentHint()
    {
        if (currentHintAnimation != null)
        {
            currentHintAnimation.StopAnimation();
        }

        OnHintComplete();
    }

    /// <summary>
    /// Hiển thị hint cho tetris block - từ block đến vị trí hợp lệ gần nhất
    /// </summary>
    /// <param name="fromWorldPos">Vị trí bắt đầu (world position của tetris block)</param>
    /// <param name="toWorldPos">Vị trí kết thúc (vị trí hợp lệ gần nhất trên grid)</param>
    public void ShowTetrisBlockHint(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        // Kiểm tra đã hiển thị hint này chưa
        if (shownHints.Contains(HintType.TetrisBlockPlacement))
        {
            return;
        }

        // Kiểm tra có hint đang hiển thị không
        if (isShowingHint)
        {
            return;
        }

        // Đánh dấu đã hiển thị
        shownHints.Add(HintType.TetrisBlockPlacement);

        // Spawn hint
        StartHint(fromWorldPos, toWorldPos);
    }

    /// <summary>
    /// Hiển thị hint cho block expand grid - từ block đến vị trí đất liền gần nhất
    /// </summary>
    /// <param name="fromWorldPos">Vị trí bắt đầu (world position của block expand grid)</param>
    /// <param name="toWorldPos">Vị trí kết thúc (đất liền gần nhất)</param>
    public void ShowBlockExpandGridHint(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        // Kiểm tra đã hiển thị hint này chưa
        if (shownHints.Contains(HintType.BlockExpandGridPlacement))
        {
            return;
        }

        // Kiểm tra có hint đang hiển thị không
        if (isShowingHint)
        {
            return;
        }

        // Đánh dấu đã hiển thị
        shownHints.Add(HintType.BlockExpandGridPlacement);

        // Spawn hint
        StartHint(fromWorldPos, toWorldPos);
    }

    /// <summary>
    /// Bắt đầu hiển thị hint animation
    /// </summary>
    private void StartHint(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        if (hintDotPrefab == null)
        {
            Debug.LogWarning("TutorialHintManager: hintDotPrefab is null!");
            return;
        }

        isShowingHint = true;

        // Spawn hint prefab
        currentHintObject = Instantiate(hintDotPrefab);

        // Set up animation component
        currentHintAnimation = currentHintObject.GetComponent<TutorialHintAnimation>();
        if (currentHintAnimation != null)
        {
            currentHintAnimation.Initialize(fromWorldPos, toWorldPos, hintDuration, OnHintComplete);
        }
        else
        {
            Debug.LogWarning("TutorialHintManager: hintDotPrefab doesn't have TutorialHintAnimation component!");
            Destroy(currentHintObject);
            currentHintObject = null;
            currentHintAnimation = null;
            isShowingHint = false;
        }
    }

    /// <summary>
    /// Callback khi hint animation hoàn thành
    /// </summary>
    private void OnHintComplete()
    {
        if (currentHintObject != null)
        {
            Destroy(currentHintObject);
            currentHintObject = null;
        }

        currentHintAnimation = null;
        isShowingHint = false;

        // Trigger event để thông báo hint đã hoàn thành
        OnHintCompleted?.Invoke();
    }

    /// <summary>
    /// Reset tất cả hints - dùng khi bắt đầu game mới
    /// </summary>
    public void ResetAllHints()
    {
        shownHints.Clear();

        if (currentHintAnimation != null)
        {
            currentHintAnimation.StopAnimation();
        }

        if (currentHintObject != null)
        {
            Destroy(currentHintObject);
            currentHintObject = null;
        }

        currentHintAnimation = null;
        isShowingHint = false;
    }

    /// <summary>
    /// Kiểm tra hint đã được hiển thị chưa
    /// </summary>
    public bool HasShownHint(HintType hintType)
    {
        return shownHints.Contains(hintType);
    }

    /// <summary>
    /// Kiểm tra có đang hiển thị hint không
    /// </summary>
    public bool IsShowingHint()
    {
        return isShowingHint;
    }
}
