using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý toàn bộ game flow: turn, item, win/lose, spawn
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 100;
    [SerializeField] private int itemsPerTurn = 3;
    [SerializeField] private int winRewardMoney = 500; // Tiền nhận được khi thắng game

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Base playerBase;
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameObject tetrisBlockPrefab;
    [SerializeField] private Transform tetrisBlockParent;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI turnText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverText;
    [SerializeField] private UnityEngine.UI.Button nextTurnButton;
    [SerializeField] private ResultUI resultUI; // Màn hình kết thúc

    [Header("Events")]
    public UnityEvent<int> OnTurnChanged;
    public UnityEvent OnGameWon;
    public UnityEvent OnGameLost;

    private int currentTurn = 1;
    private bool isGameActive = false;
    private bool isPlayerTurn = true; // Phase người chơi đặt item
    private List<GameObject> currentTurnSpawnedObjects = new List<GameObject>(); // TetrisBlock hoặc BlockExpandGrid
    private List<TetrisBlock> placedBlocks = new List<TetrisBlock>();

    private void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// Khởi tạo game
    /// </summary>
    private void InitializeGame()
    {
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        if (playerBase == null) playerBase = FindFirstObjectByType<Base>();
        if (itemSpawner == null) itemSpawner = FindFirstObjectByType<ItemSpawner>();
        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();

        // Subscribe events
        if (playerBase != null)
        {
            playerBase.OnBaseDestroyed.AddListener(OnBaseDestroyed);
        }

        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(EndPlayerPhase);
        }

        isGameActive = true;
        StartNewTurn();
    }

    /// <summary>
    /// Bắt đầu turn mới
    /// </summary>
    private void StartNewTurn()
    {
        if (!isGameActive) return;

        // Kiểm tra win condition
        if (currentTurn > maxTurns)
        {
            WinGame();
            return;
        }

        // Cập nhật UI
        UpdateTurnUI();
        OnTurnChanged?.Invoke(currentTurn);

        // Phase 1: Người chơi nhận item và đặt
        StartPlayerPhase();
    }

    /// <summary>
    /// Phase người chơi: nhận prefab và đặt
    /// </summary>
    private void StartPlayerPhase()
    {
        isPlayerTurn = true;

        // Xóa prefab turn trước (nếu còn)
        ClearPreviousTurnObjects();

        // Spawn prefab mới (TetrisBlock hoặc BlockExpandGrid)
        if (itemSpawner != null)
        {
            currentTurnSpawnedObjects = itemSpawner.SpawnItems(itemsPerTurn);
        }

        // Enable button next turn
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = true;
        }
    }

    /// <summary>
    /// Kết thúc phase người chơi, bắt đầu phase quái
    /// </summary>
    public void EndPlayerPhase()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;

        // Xóa các prefab chưa dùng (chưa placed)
        ClearUnusedObjects();

        // Disable button
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = false;
        }

        // Bắt đầu phase quái
        StartEnemyPhase();
    }

    /// <summary>
    /// Phase quái: spawn và di chuyển
    /// </summary>
    private void StartEnemyPhase()
    {
        // Spawn quái
        if (enemySpawner != null)
        {
            enemySpawner.SpawnEnemiesForTurn(currentTurn);
        }

        // Đợi turn kết thúc
        StartCoroutine(WaitForTurnEnd());
    }

    /// <summary>
    /// Đợi turn kết thúc
    /// </summary>
    private IEnumerator WaitForTurnEnd()
    {
        // Đợi enemy spawning xong trước
        if (enemySpawner != null)
        {
            while (enemySpawner.IsSpawning())
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Sau đó đợi tất cả enemy bị tiêu diệt
        bool isEnemyRemaining = true;

        while (isEnemyRemaining)
        {
            isEnemyRemaining = false;
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null && enemy.IsAlive())
                {
                    isEnemyRemaining = true;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        currentTurn++;
        StartNewTurn();
    }

    /// <summary>
    /// Spawn khối Tetris (từ item) - UI Canvas
    /// </summary>
    public void SpawnTetrisBlock(TetrisBlockType blockType)
    {
        if (tetrisBlockPrefab == null) return;

        // Spawn trong UI Canvas (cần parent là Canvas hoặc Grid)
        GameObject blockObj = Instantiate(tetrisBlockPrefab, tetrisBlockParent);

        TetrisBlock block = blockObj.GetComponent<TetrisBlock>();
        if (block != null)
        {
            block.Initialize(blockType, 1); // Level 1 ban đầu

            // Đặt vị trí spawn (có thể là vị trí item hoặc vị trí spawn area)
            // RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            // if (blockRect != null && grid != null)
            // {
            //     blockRect.anchoredPosition = grid.GridToUIPosition(Vector2Int.zero);
            // }

            placedBlocks.Add(block);
        }
    }

    /// <summary>
    /// Xóa prefab turn trước
    /// </summary>
    private void ClearPreviousTurnObjects()
    {
        foreach (GameObject obj in currentTurnSpawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentTurnSpawnedObjects.Clear();
    }

    /// <summary>
    /// Xóa các prefab chưa được sử dụng (chưa placed hoặc chưa used)
    /// </summary>
    private void ClearUnusedObjects()
    {
        // Xóa TetrisBlock chưa placed
        TetrisBlock[] allBlocks = FindObjectsByType<TetrisBlock>(FindObjectsSortMode.None);
        foreach (TetrisBlock block in allBlocks)
        {
            if (block != null && !block.IsPlaced())
            {
                Destroy(block.gameObject);
            }
        }

        // Xóa BlockExpandGrid chưa used
        BlockExpandGrid[] allExpands = FindObjectsByType<BlockExpandGrid>(FindObjectsSortMode.None);
        foreach (BlockExpandGrid expand in allExpands)
        {
            if (expand != null && !expand.IsUsed())
            {
                Destroy(expand.gameObject);
            }
        }

        currentTurnSpawnedObjects.Clear();
    }

    /// <summary>
    /// Cập nhật UI turn
    /// </summary>
    private void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = $"Turn: {currentTurn}/{maxTurns}";
        }
    }

    /// <summary>
    /// Thành bị phá hủy
    /// </summary>
    private void OnBaseDestroyed()
    {
        LoseGame();
    }

    /// <summary>
    /// Thắng game
    /// </summary>
    private void WinGame()
    {
        isGameActive = false;

        // Hiển thị màn hình thắng
        if (resultUI != null)
        {
            resultUI.ShowWin();
        }
        else if (gameOverText != null)
        {
            gameOverText.text = "Victory!";
            gameOverText.gameObject.SetActive(true);
        }

        // Thêm tiền thưởng
        if (TowerDataManager.Instance != null)
        {
            TowerDataManager.Instance.AddMoney(winRewardMoney);
        }

        OnGameWon?.Invoke();
    }

    /// <summary>
    /// Thua game
    /// </summary>
    private void LoseGame()
    {
        isGameActive = false;

        // Hiển thị màn hình thua
        if (resultUI != null)
        {
            resultUI.ShowLose();
        }
        else if (gameOverText != null)
        {
            gameOverText.text = "Game Over!";
            gameOverText.gameObject.SetActive(true);
        }

        OnGameLost?.Invoke();
    }

    // Getters
    public int GetCurrentTurn() => currentTurn;
    public bool IsGameActive() => isGameActive;
    public bool IsPlayerTurn() => isPlayerTurn;
}

