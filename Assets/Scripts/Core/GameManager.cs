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
            Debug.Log($"랜덤 직업 선택: {randomJob.jobName}");
        }

        return randomJob;
    }
}