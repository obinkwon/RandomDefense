using UnityEngine;

// 스테이지 진행: 1분마다 다음 스테이지로 넘어가고, 못 잡은 적은 맵에 남아 쌓인다.
// 적 구성과 체력은 밸런스 시트 "스테이지 적"을 따른다.
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private DifficultyData difficulty;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Enemies (10스테이지 단위)")]
    [Tooltip("1~10, 11~20 ... 구간마다 나오는 일반 적")]
    [SerializeField] private EnemyData[] normalEnemies;
    [Tooltip("10, 20 ... 스테이지에 나오는 보스")]
    [SerializeField] private EnemyData[] bosses;

    [Header("Rules")]
    [SerializeField] private float stageDuration = 60f;
    [SerializeField] private int enemyLimit = 100;
    [SerializeField] private float lifeLossInterval = 10f;

    private int currentStage;
    private float stageTimer;
    private float lifeLossTimer;

    public int CurrentStage => currentStage;
    public int MaxStage => difficulty.maxStage;
    public float RemainingTime => stageTimer;
    public int EnemyLimit => enemyLimit;
    public bool IsLastStage => currentStage >= difficulty.maxStage;

    private void Awake()
    {
        Instance = this;
    }

    public void SetDifficulty(DifficultyData data)
    {
        difficulty = data;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (currentStage == 0)
        {
            StartNextStage();
            return;
        }

        UpdateLifeLoss();

        if (IsLastStage && !enemySpawner.IsSpawning && Enemy.Alive.Count == 0)
        {
            GameManager.Instance.ClearGame();
            return;
        }

        if (stageTimer <= 0f)
            return;

        stageTimer -= Time.deltaTime;

        if (stageTimer > 0f)
            return;

        // 보스 스테이지 제한 시간 안에 보스를 못 잡으면 패배 (F-09).
        if (IsBossAlive())
        {
            GameManager.Instance.GameOver();
            return;
        }

        // 마지막 스테이지는 남은 적을 다 잡을 때까지 기다린다.
        if (!IsLastStage)
            StartNextStage();
    }

    private void StartNextStage()
    {
        currentStage++;
        stageTimer = stageDuration;

        int band = Mathf.Min((currentStage - 1) / 10, normalEnemies.Length - 1);
        float hpMul = difficulty.hpMultiplier;
        float speedMul = difficulty.speedMultiplier;

        enemySpawner.Spawn(normalEnemies[band], EnemyCountFor(currentStage), HpFor(currentStage, false) * hpMul, speedMul);

        if (IsBossStage(currentStage))
            enemySpawner.Spawn(bosses[band], 1, HpFor(currentStage, true) * hpMul, speedMul);
    }

    // 1~40스테이지 30마리, 41스테이지부터 50마리 (F-07).
    public static int EnemyCountFor(int stage)
    {
        return stage <= 40 ? 30 : 50;
    }

    public static bool IsBossStage(int stage)
    {
        return stage % 10 == 0;
    }

    // 시트 공식: ROUND(50 × 1.1^(스테이지-1) × 보스면 20), 보통 난이도 기준.
    public static float HpFor(int stage, bool boss)
    {
        return Mathf.Round(50f * Mathf.Pow(1.1f, stage - 1) * (boss ? 20f : 1f));
    }

    private static bool IsBossAlive()
    {
        foreach (Enemy enemy in Enemy.Alive)
        {
            if (enemy.Data.isBoss)
                return true;
        }

        return false;
    }

    private void UpdateLifeLoss()
    {
        if (CountTowardLimit() < enemyLimit)
        {
            lifeLossTimer = 0f;
            return;
        }

        lifeLossTimer += Time.deltaTime;

        if (lifeLossTimer >= lifeLossInterval)
        {
            lifeLossTimer -= lifeLossInterval;
            GameManager.Instance.LoseLife();
        }
    }

    public int CountTowardLimit()
    {
        int count = 0;

        foreach (Enemy enemy in Enemy.Alive)
        {
            if (!enemy.Data.isBoss)
                count++;
        }

        return count;
    }
}
