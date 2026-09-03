using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private float waveInterval = 3f;
    [SerializeField] private int baseEnemiesPerWave = 1;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    public int CurrentWave => currentWave;

    private float waveTimer;

    private void Start()
    {
        waveTimer = waveInterval;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            StartNextWave();
            waveTimer = waveInterval;
        }
    }

    private void StartNextWave()
    {
        currentWave++;

        int enemiesPerWave =
            baseEnemiesPerWave + currentWave - 1;

        Debug.Log(
            $"Wave {currentWave} 시작 - 적 {enemiesPerWave}마리"
        );

        if (enemySpawner != null)
        {
            enemySpawner.SpawnWave(
                currentWave,
                enemiesPerWave
            );
        }
    }
}