using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// 인게임 HUD: 위쪽 카운터, 오른쪽 위 진행 버튼, 아래쪽 소환·조합·조합표 버튼.
public class GameUI : MonoBehaviour
{
    [Header("Counters (위쪽)")]
    [SerializeField] private TMP_Text crystalText;
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text enemyText;

    [Header("Buttons")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button fastButton;
    [SerializeField] private Button combineButton;
    [SerializeField] private Button tableButton;

    [Header("Panels")]
    [SerializeField] private CombinePanel combinePanel;
    [SerializeField] private CombinePanel tablePanel;

    [Header("Pause Icon")]
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    [Header("Speed Icon")]
    [SerializeField] private Sprite fastSprite;
    [SerializeField] private Sprite normalSprite;

    private void Awake()
    {
        // 씬에 EventSystem이 없으면 버튼이 눌리지 않는다. 새 Input System용 모듈로 만든다.
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private void Start()
    {
        summonButton.onClick.AddListener(() => SummonManager.Instance.TrySummon());
        pauseButton.onClick.AddListener(TogglePause);
        fastButton.onClick.AddListener(() => GameManager.Instance.SetFast(!GameManager.Instance.IsFast));
        combineButton.onClick.AddListener(combinePanel.Open);
        tableButton.onClick.AddListener(tablePanel.Open);
    }

    private void Update()
    {
        GameManager game = GameManager.Instance;
        StageManager stage = StageManager.Instance;

        crystalText.text = ManaCrystalManager.Instance.CurrentCrystals.ToString();
        lifeText.text = game.Life.ToString();

        float time = Mathf.Max(0f, stage.RemainingTime);
        stageText.text = $"{stage.CurrentStage}/{stage.MaxStage}";
        timeText.text = $"{(int)time / 60}:{(int)time % 60:00}";
        enemyText.text = $"{stage.CountTowardLimit()}/{stage.EnemyLimit}";

        pauseButton.image.sprite = game.CurrentState == GameState.Paused ? playSprite : pauseSprite;
        // 버튼은 누르면 바뀔 상태를 보여준다: 빨리 감기 중이면 일반 속도 아이콘
        fastButton.image.sprite = game.IsFast ? normalSprite : fastSprite;
        // 만들 수 있는 조합이 있으면 조합 버튼을 밝게
        combineButton.image.color = SummonManager.Instance.MakeableRecipes().Count > 0 ? Color.white : new Color(0.55f, 0.55f, 0.55f);
    }

    private void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Paused)
            GameManager.Instance.ResumeGame();
        else
            GameManager.Instance.PauseGame();
    }
}
