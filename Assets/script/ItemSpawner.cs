using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn prefab random mỗi turn
/// Tỷ lệ: 70% Tetris Block, 30% Grid Expansion
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject tetrisBlockPrefab; // Prefab của TetrisBlock
    [SerializeField] private GameObject blockExpandGridPrefab; // Prefab của BlockExpandGrid
    [SerializeField] private Transform spawnContainer;
    [SerializeField] private float tetrisBlockChance = 0.7f; // 70%

    [Header("Spawn Positions")]
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();

    [Header("Tutorial Hint")]
    [SerializeField] private float hintDelayAfterSpawn = 0.5f; // Delay trước khi hiển thị hint
    [SerializeField] private float delayBetweenHints = 0.5f; // Delay giữa 2 hints khi cả 2 loại xuất hiện

    private TutorialHintManager hintManager;
    private Grid grid;
    private GameObject pendingTetrisBlock = null; // TetrisBlock đang chờ hiển thị hint
    private GameObject pendingBlockExpandGrid = null; // BlockExpandGrid đang chờ hiển thị hint

    private void Awake()
    {
        hintManager = FindFirstObjectByType<TutorialHintManager>();
        grid = FindFirstObjectByType<Grid>();
    }

    private void OnEnable()
    {
        // Đăng ký event khi hint hoàn thành
        if (hintManager != null)
        {
            hintManager.OnHintCompleted += OnHintCompleted;
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký event
        if (hintManager != null)
        {
            hintManager.OnHintCompleted -= OnHintCompleted;
        }
    }

    /// <summary>
    /// Spawn 3 prefab random (TetrisBlock hoặc BlockExpandGrid)
    /// </summary>
    public List<GameObject> SpawnItems(int count = 3)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = SpawnRandomPrefab();
            if (obj != null)
            {
                spawnedObjects.Add(obj);

                // Đặt vị trí spawn (3D)
                if (i < spawnPositions.Count && spawnPositions[i] != null)
                {
                    obj.transform.position = spawnPositions[i].position;
                }
            }
        }

        // Kiểm tra và hiển thị tutorial hint cho mỗi loại item lần đầu tiên
        CheckAndShowHints(spawnedObjects);

        return spawnedObjects;
    }

    /// <summary>
    /// Spawn một prefab random
    /// </summary>
    private GameObject SpawnRandomPrefab()
    {
        float random = Random.Range(0f, 1f);
        GameObject prefabToSpawn = null;

        if (random < tetrisBlockChance)
        {
            // Spawn Tetris Block
            prefabToSpawn = tetrisBlockPrefab;
        }
        else
        {
            // Spawn BlockExpandGrid
            prefabToSpawn = blockExpandGridPrefab;
        }

        if (prefabToSpawn == null) return null;

        Transform parent = spawnContainer != null ? spawnContainer : transform;
        GameObject obj = Instantiate(prefabToSpawn, parent);

        // Nếu là TetrisBlock, random type
        TetrisBlock block = obj.GetComponent<TetrisBlock>();
        if (block != null)
        {
            TetrisBlockType randomType = GetRandomTetrisBlockType();
            block.Initialize(randomType, 1);
        }

        return obj;
    }

    /// <summary>
    /// Random một loại khối Tetris
    /// </summary>
    private TetrisBlockType GetRandomTetrisBlockType()
    {
        System.Array values = System.Enum.GetValues(typeof(TetrisBlockType));
        return (TetrisBlockType)values.GetValue(Random.Range(0, values.Length));
    }

    /// <summary>
    /// Kiểm tra và hiển thị hint cho mỗi loại item lần đầu tiên xuất hiện
    /// </summary>
    private void CheckAndShowHints(List<GameObject> spawnedObjects)
    {
        if (hintManager == null || grid == null || spawnedObjects.Count == 0)
        {
            return;
        }

        // Tìm TetrisBlock và BlockExpandGrid cần hiển thị hint
        GameObject tetrisBlockToShow = null;
        GameObject blockExpandGridToShow = null;

        // Tìm TetrisBlock đầu tiên nếu hint chưa được hiển thị
        if (!hintManager.HasShownHint(TutorialHintManager.HintType.TetrisBlockPlacement))
        {
            foreach (GameObject obj in spawnedObjects)
            {
                TetrisBlock tetrisBlock = obj.GetComponent<TetrisBlock>();
                if (tetrisBlock != null)
                {
                    tetrisBlockToShow = obj;
                    break;
                }
            }
        }

        // Tìm BlockExpandGrid đầu tiên nếu hint chưa được hiển thị
        if (!hintManager.HasShownHint(TutorialHintManager.HintType.BlockExpandGridPlacement))
        {
            foreach (GameObject obj in spawnedObjects)
            {
                BlockExpandGrid blockExpandGrid = obj.GetComponent<BlockExpandGrid>();
                if (blockExpandGrid != null)
                {
                    blockExpandGridToShow = obj;
                    break;
                }
            }
        }

        // Nếu có cả 2 loại cần hiển thị hint
        if (tetrisBlockToShow != null && blockExpandGridToShow != null)
        {
            // Hiển thị TetrisBlock trước, lưu BlockExpandGrid để hiển thị sau
            pendingTetrisBlock = null;
            pendingBlockExpandGrid = blockExpandGridToShow;
            StartCoroutine(ShowTetrisBlockHint(tetrisBlockToShow));
        }
        // Chỉ có TetrisBlock
        else if (tetrisBlockToShow != null)
        {
            pendingTetrisBlock = null;
            pendingBlockExpandGrid = null;
            StartCoroutine(ShowTetrisBlockHint(tetrisBlockToShow));
        }
        // Chỉ có BlockExpandGrid
        else if (blockExpandGridToShow != null)
        {
            pendingTetrisBlock = null;
            pendingBlockExpandGrid = null;
            StartCoroutine(ShowBlockExpandGridHint(blockExpandGridToShow));
        }
    }

    /// <summary>
    /// Callback khi hint hoàn thành - hiển thị hint pending nếu có
    /// </summary>
    private void OnHintCompleted()
    {
        // Kiểm tra xem có pending hint nào không
        if (pendingBlockExpandGrid != null)
        {
            GameObject objToShow = pendingBlockExpandGrid;
            pendingBlockExpandGrid = null;
            StartCoroutine(ShowBlockExpandGridHintDelayed(objToShow));
        }
        else if (pendingTetrisBlock != null)
        {
            GameObject objToShow = pendingTetrisBlock;
            pendingTetrisBlock = null;
            StartCoroutine(ShowTetrisBlockHintDelayed(objToShow));
        }
    }

    /// <summary>
    /// Hiển thị TetrisBlock hint với delay
    /// </summary>
    private IEnumerator ShowTetrisBlockHintDelayed(GameObject tetrisBlockObj)
    {
        yield return new WaitForSeconds(delayBetweenHints);
        yield return ShowTetrisBlockHint(tetrisBlockObj);
    }

    /// <summary>
    /// Hiển thị BlockExpandGrid hint với delay
    /// </summary>
    private IEnumerator ShowBlockExpandGridHintDelayed(GameObject blockExpandGridObj)
    {
        yield return new WaitForSeconds(delayBetweenHints);
        yield return ShowBlockExpandGridHint(blockExpandGridObj);
    }

    /// <summary>
    /// Hiển thị tutorial hint cho TetrisBlock
    /// </summary>
    private IEnumerator ShowTetrisBlockHint(GameObject tetrisBlockObj)
    {
        if (hintManager == null || grid == null || tetrisBlockObj == null)
        {
            yield break;
        }

        // Đợi một chút trước khi hiển thị hint
        yield return new WaitForSeconds(hintDelayAfterSpawn);

        TetrisBlock tetrisBlock = tetrisBlockObj.GetComponent<TetrisBlock>();
        if (tetrisBlock == null) yield break;

        Vector3 fromPos = tetrisBlockObj.transform.position;
        BlockShape shape = TetrisBlockShapes.GetShape(tetrisBlock.GetBlockType());
        Vector3 toPos = grid.FindNearestValidPlacementForBlock(fromPos, shape);

        if (toPos != Vector3.zero)
        {
            hintManager.ShowTetrisBlockHint(fromPos, toPos);
        }
    }

    /// <summary>
    /// Hiển thị tutorial hint cho BlockExpandGrid
    /// </summary>
    private IEnumerator ShowBlockExpandGridHint(GameObject blockExpandGridObj)
    {
        if (hintManager == null || grid == null || blockExpandGridObj == null)
        {
            yield break;
        }

        // Đợi một chút trước khi hiển thị hint
        yield return new WaitForSeconds(hintDelayAfterSpawn);

        BlockExpandGrid blockExpandGrid = blockExpandGridObj.GetComponent<BlockExpandGrid>();
        if (blockExpandGrid == null) yield break;

        Vector3 fromPos = blockExpandGridObj.transform.position;
        Vector3 toPos = grid.FindNearestNonGridCellNearValidGrid(fromPos);

        if (toPos != Vector3.zero)
        {
            hintManager.ShowBlockExpandGridHint(fromPos, toPos);
        }
    }
}


