using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private Enemy enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;

    public void SpawnWave(int wave)
    {
        Debug.Log($"EnemySpawner: Wave {wave} 적 생성 시작");

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 설정되지 않았습니다.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point가 설정되지 않았습니다.", this);
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogError("Target Point가 설정되지 않았습니다.", this);
            return;
        }

        Enemy enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        enemy.SetTarget(targetPoint);

        Debug.Log($"Enemy 생성 완료: {enemy.name}");
    }
}