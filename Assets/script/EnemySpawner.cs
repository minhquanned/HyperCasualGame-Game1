using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn quái mỗi turn, có hệ thống scaling:
/// - Số lượng quái tăng theo turn
/// - Máu + Damage + Speed tăng theo turn
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PathManager pathManager;
    [SerializeField] private Transform enemyParent;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 0.3f;

    [Header("Enemy Count Scaling")]
    [SerializeField] private int baseEnemyCount = 3;        // số quái turn 1
    [SerializeField] private int enemyIncreasePerTurn = 2;  // mỗi turn tăng thêm bao nhiêu quái

    [Header("Enemy Health Scaling")]
    [SerializeField] private float baseHealth = 50f;        // máu turn 1
    [SerializeField] private float healthIncreasePerTurn = 15f;

    [Header("Enemy Damage Scaling")]
    [SerializeField] private float baseDamage = 5f;         // damage turn 1
    [SerializeField] private float damageIncreasePerTurn = 1f;

    [Header("Enemy Speed Scaling")]
    [SerializeField] private float baseSpeed = 50f;         // speed turn 1
    [SerializeField] private float speedIncreasePerTurn = 1.2f;

    private int currentTurn = 1;


    // ====================================================================== //
    //                          PUBLIC API
    // ====================================================================== //

    /// <summary>
    /// Gọi khi bắt đầu turn mới
    /// </summary>
    public void SpawnEnemiesForTurn(int turnNumber)
    {
        currentTurn = turnNumber;
        StartCoroutine(SpawnEnemiesCoroutine());
    }


    // ====================================================================== //
    //                          STATS CALCULATOR 
    // ====================================================================== //

    private int GetEnemyCount(int turn)
    {
        return Mathf.Max(1, baseEnemyCount + (turn - 1) * enemyIncreasePerTurn);
    }

    private float GetEnemyHealth(int turn)
    {
        return baseHealth + (turn - 1) * healthIncreasePerTurn;
    }

    private float GetEnemyDamage(int turn)
    {
        return baseDamage + (turn - 1) * damageIncreasePerTurn;
    }

    private float GetEnemySpeed(int turn)
    {
        return baseSpeed + (turn - 1) * speedIncreasePerTurn;
    }


    // ====================================================================== //
    //                          SPAWN LOGIC
    // ====================================================================== //

    private IEnumerator SpawnEnemiesCoroutine()
    {
        if (enemyPrefab == null || pathManager == null)
            yield break;

        List<Vector3> waypoints = pathManager.GetWaypoints();
        if (waypoints == null || waypoints.Count == 0)
            yield break;

        int enemyCount = GetEnemyCount(currentTurn);
        float HP = GetEnemyHealth(currentTurn);
        float DMG = GetEnemyDamage(currentTurn);
        float SPD = GetEnemySpeed(currentTurn);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, enemyParent);

            // Đặt vị trí spawn (waypoint đầu) - 3D
            if (waypoints.Count > 0)
            {
                enemyObj.transform.position = waypoints[0];
            }

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(waypoints);
                enemy.SetStats(HP, DMG, SPD);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
