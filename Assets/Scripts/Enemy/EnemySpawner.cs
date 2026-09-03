using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private Enemy enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float spawnInterval = 0.5f;

    public void SpawnWave(int wave, int enemyCount)
    {
        Debug.Log(
            $"EnemySpawner: Wave {wave} 적 생성 시작 - {enemyCount}마리"
        );

        StartCoroutine(SpawnEnemies(enemyCount));
    }

    private System.Collections.IEnumerator SpawnEnemies(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();

            if (i < enemyCount - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError(
                "Enemy Prefab이 설정되지 않았습니다.",
                this
            );
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "Spawn Point가 설정되지 않았습니다.",
                this
            );
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogError(
                "Target Point가 설정되지 않았습니다.",
                this
            );
            return;
        }

        Enemy enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        enemy.SetTarget(targetPoint);

        Debug.Log(
            $"Enemy 생성 완료: {enemy.name}"
        );
    }
}