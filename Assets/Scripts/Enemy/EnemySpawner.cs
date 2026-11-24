using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;

    [Tooltip("순환 경로. 마지막 지점 다음은 첫 지점으로 이어진다. 첫 지점에서 생성된다.")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float spawnInterval = 0.5f;

    private int runningSpawns;

    public bool IsSpawning => runningSpawns > 0;

    public void Spawn(EnemyData data, int count, float hp, float speedMultiplier)
    {
        if (enemyPrefab == null || waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: Enemy Prefab 또는 Waypoints가 설정되지 않았습니다.", this);
            return;
        }

        StartCoroutine(SpawnEnemies(data, count, hp, speedMultiplier));
    }

    private IEnumerator SpawnEnemies(EnemyData data, int count, float hp, float speedMultiplier)
    {
        runningSpawns++;

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
            enemy.Init(data, waypoints, hp, speedMultiplier);

            yield return new WaitForSeconds(spawnInterval);
        }

        runningSpawns--;
    }
}
