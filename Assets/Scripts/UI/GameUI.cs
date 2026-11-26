using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// 인게임 HUD: 위쪽 카운터, 오른쪽 위 진행 버튼, 아래쪽 조합표·소환·조합(·취소) 버튼.
// 조합: 조합 버튼으로 조합 모드에 들어가 필드 직업을 눌러 재료를 고르고(최대 4), "조합하기"로 조합한다.
// 조합 모드 동안 조합 버튼은 현판(btn_combine_mode)으로 바뀌어 깜빡인다 (공통 이펙트 시안 "선택·사거리·조합 모드 미리보기").
// 조합표를 열어도 조합 모드는 유지되고, 열려 있는 동안 조합·취소 버튼과 필드 선택만 막힌다.
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
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button tableButton;

    [Header("Panels")]
    [SerializeField] private RecipeTablePanel tablePanel;
    [Tooltip("잠깐 떴다 사라지는 안내 글자")]
    [SerializeField] private TMP_Text toastText;

    [Header("Combine Mode")]
    [Tooltip("조합 모드일 때 조합 버튼 그림 (현판)")]
    [SerializeField] private Sprite combineModeSprite;

    [Header("Pause Icon")]
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    private TMP_Text combineLabel;
    private Sprite combineSprite;
    private float toastUntil;

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
        combineButton.onClick.AddListener(OnCombine);
        cancelButton.onClick.AddListener(() => SummonManager.Instance.SetCombineMode(false));
        tableButton.onClick.AddListener(tablePanel.Toggle);
        combineLabel = combineButton.GetComponentInChildren<TMP_Text>();
        combineSprite = combineButton.image.sprite;
        toastText.gameObject.SetActive(false);
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

        SummonManager summon = SummonManager.Instance;
        bool tableOpen = tablePanel.IsOpen;
        combineLabel.text = summon.CombineMode ? $"조합하기 {summon.PickCount}/{SummonManager.MaxPicks}" : "조합";
        cancelButton.gameObject.SetActive(summon.CombineMode);
        combineButton.interactable = !tableOpen;
        cancelButton.interactable = !tableOpen;
        // 조합 모드면 현판 그림으로 바꾸고 0.6초마다 깜빡인다. 평소에는 조합표 버튼과 같은 돌 테두리.
        combineButton.image.sprite = summon.CombineMode ? combineModeSprite : combineSprite;
        combineButton.image.type = summon.CombineMode ? Image.Type.Simple : Image.Type.Sliced;
        bool dim = summon.CombineMode && (int)(Time.unscaledTime / 0.6f) % 2 == 1;
        combineButton.image.color = dim ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
        tableButton.image.color = tableOpen ? new Color(1f, 0.85f, 0.4f) : Color.white;

        if (toastText.gameObject.activeSelf && Time.unscaledTime > toastUntil)
            toastText.gameObject.SetActive(false);
    }

    private void Toast(string message)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        toastUntil = Time.unscaledTime + 2f;
    }

    private void OnCombine()
    {
        SummonManager summon = SummonManager.Instance;
        if (!summon.CombineMode)
            summon.SetCombineMode(true);
        else if (summon.PickCount < 2)
            Toast("재료를 2개 이상 고르세요");
        else if (!summon.CombinePicked())
            Toast("이 재료로 만들 수 있는 조합이 없어요");
    }

    private void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Paused)
            GameManager.Instance.ResumeGame();
        else
            GameManager.Instance.PauseGame();
    }
}
