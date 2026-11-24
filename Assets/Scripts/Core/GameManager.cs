using UnityEngine;

// 게임 상태, 라이프, 배속·일시정지를 관리한다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState currentState = GameState.Ready;
    [SerializeField] private int startLife = 10;

    [Header("Speed (천천히 / 빨리)")]
    [SerializeField] private float slowSpeed = 1f;
    [SerializeField] private float fastSpeed = 2f;

    private int life;
    private bool isFast;

    public GameState CurrentState => currentState;
    public int Life => life;
    public bool IsFast => isFast;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        life = startLife;
        currentState = GameState.Playing;
        ApplyTimeScale();
    }

    public void SetFast(bool fast)
    {
        isFast = fast;
        ApplyTimeScale();
    }

    // ponytail: 멀티 미구현이라 일시정지를 항상 허용. 멀티 추가 시 싱글에서만 허용 (F-12).
    public void PauseGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
            return;

        currentState = GameState.Playing;
        ApplyTimeScale();
    }

    public void LoseLife()
    {
        if (currentState != GameState.Playing)
            return;

        life--;

        if (life <= 0)
            GameOver();
    }

    public void ClearGame()
    {
        currentState = GameState.Cleared;
        Time.timeScale = 0f;
        Debug.Log("클리어");
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("게임 오버");
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = isFast ? fastSpeed : slowSpeed;
    }
}
