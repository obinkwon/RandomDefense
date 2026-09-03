using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField]
    private GameState currentState = GameState.Ready;

    [Header("Jobs")]
    [SerializeField]
    private JobData[] availableJobs;

    [Header("Character")]
    [SerializeField]
    private Character characterPrefab;

    [SerializeField]
    private Transform[] characterSpawnPoints;

    public GameState CurrentState => currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentState = GameState.Playing;

        Debug.Log("게임 시작");
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;

        Debug.Log("게임 일시정지");
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
            return;

        currentState = GameState.Playing;
        Time.timeScale = 1f;

        Debug.Log("게임 재개");
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        Debug.Log("게임 오버");
    }

    public JobData GetRandomJob()
    {
        JobData randomJob = JobFactory.GetRandomJob(availableJobs);

        if (randomJob != null)
        {
            Debug.Log(
                $"랜덤 직업 선택: {randomJob.jobName}"
            );
        }

        return randomJob;
    }

    private void SpawnCharacter()
    {
        if (characterPrefab == null)
        {
            Debug.LogError(
                "Character Prefab이 설정되지 않았습니다.",
                this
            );
            return;
        }

        if (characterSpawnPoints == null ||
            characterSpawnPoints.Length == 0)
        {
            Debug.LogError(
                "Character Spawn Point가 설정되지 않았습니다.",
                this
            );
            return;
        }

        Transform spawnPoint = GetAvailableSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.Log("사용 가능한 타워 슬롯이 없습니다.");
            return;
        }

        Character character = Instantiate(
            characterPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log(
            $"Character 생성 완료: {character.name}"
        );
    }

    public bool TrySpawnCharacter()
    {
        if (GameManager.Instance == null)
            return false;

        if (currentState != GameState.Playing)
            return false;

        if (GoldManager.Instance == null)
        {
            Debug.LogError("GoldManager을 찾을 수 없습니다.");
            return false;
        }

        Transform spawnPoint = GetAvailableSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.Log("사용 가능한 타워 슬롯이 없습니다.");
            return false;
        }

        if (!GoldManager.Instance.SpendCharacterCost())
        {
            return false;
        }

        Character character = Instantiate(
            characterPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log(
            $"Character 생성 완료: {character.name}"
        );

        return true;
    }

    private void Update()
    {

    }

    public void SummonCharacter()
    {
        TrySpawnCharacter();
    }

    private Transform GetAvailableSpawnPoint()
    {
        foreach (Transform spawnPoint in characterSpawnPoints)
        {
            if (spawnPoint == null)
                continue;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                spawnPoint.position,
                0.1f
            );

            bool occupied = false;

            foreach (Collider2D collider in colliders)
            {
                if (collider.GetComponent<Character>() != null)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                return spawnPoint;
            }
        }

        return null;
    }
}