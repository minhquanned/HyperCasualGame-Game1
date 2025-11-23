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
}

