using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 마나 결정으로 직업을 소환해 빈 칸에 자동 배치한다. 등급을 먼저 뽑고 그 등급 안에서 직업을 균등하게 뽑는다.
public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [SerializeField] private int summonCost = 6;
    [SerializeField] private SummonRates rates;
    [SerializeField] private JobData[] jobs;
    [SerializeField] private JobUnit jobUnitPrefab;
    [SerializeField] private CombinationTable combinations;
    [Tooltip("필드 가운데의 고정 배치 칸 (기본 6×3)")]
    [SerializeField] private Transform[] slots;

    [Header("선택 표시")]
    [Tooltip("일반 모드에서 누른 직업을 감싸는 꺾쇠 (unit_select)")]
    [SerializeField] private Sprite selectSprite;
    [Tooltip("사거리 원 안쪽을 옅게 채우는 원 (지름 2칸)")]
    [SerializeField] private Sprite rangeFillSprite;

    public const int MaxPicks = 4;

    private JobUnit[] occupants;
    // 일반 모드에서 고른 칸 (-1이면 없음)과 그 표시
    private int selected = -1;
    private GameObject selectMark;
    private GameObject rangeCircle;
    // 조합 모드에서 고른 칸과 그 칸의 발판
    private readonly List<int> picks = new List<int>();
    private readonly List<GameObject> pickMarks = new List<GameObject>();

    public bool CombineMode { get; private set; }
    public int PickCount => picks.Count;

    public CombinationTable Combinations => combinations;

    public int SummonCost => summonCost;
    public IReadOnlyList<JobUnit> Occupants => occupants;

    private void Awake()
    {
        Instance = this;
        occupants = new JobUnit[slots.Length];
    }

    // 필드 누르기 (마우스·터치 공통). 공통 이펙트 시안 "선택·사거리·조합 모드 미리보기" 기준.
    // 일반 모드: 직업을 누르면 꺾쇠와 사거리 원으로 선택, 같은 직업을 다시 누르거나 빈 곳을 누르면 해제.
    // 조합 모드: 직업을 누르면 재료로 고른다 (최대 4개, 다시 누르면 뺀다).
    private void Update()
    {
        // 조합·판매로 고른 직업이 사라지면 선택도 푼다.
        if (selected >= 0 && occupants[selected] == null)
            Select(-1);

        Pointer pointer = Pointer.current;
        if (pointer == null || Camera.main == null || !pointer.press.wasPressedThisFrame)
            return;

        // UI(조합표 창과 뒤 가림막 포함) 위를 누르면 필드는 반응하지 않는다.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        Vector3 world = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());
        int slot = SlotAt(world);
        bool hasUnit = slot >= 0 && occupants[slot] != null;

        if (CombineMode)
        {
            if (hasUnit)
                TogglePick(slot);
        }
        else
        {
            Select(hasUnit && slot != selected ? slot : -1);
        }
    }

    // 칸 하나를 선택한다. -1이면 선택을 푼다.
    private void Select(int slot)
    {
        selected = slot;
        if (selectMark != null) Destroy(selectMark);
        if (rangeCircle != null) Destroy(rangeCircle);
        if (slot < 0)
            return;

        Vector3 pos = slots[slot].position;

        selectMark = new GameObject("SelectMark", typeof(SpriteRenderer));
        selectMark.transform.position = pos;
        SpriteRenderer mark = selectMark.GetComponent<SpriteRenderer>();
        mark.sprite = selectSprite;
        mark.sortingOrder = 20;

        rangeCircle = RangeCircle(pos, occupants[slot].JobData.attackRange);
    }

    // 점선: 흰·투명 두 칸짜리 텍스처를 선을 따라 반복한다 (1칸에 3번). 한 번 만들어 계속 쓴다.
    private static Material dashMaterial;

    // 스프라이트 기본 재질(프로젝트 렌더러에 맞는 것)을 복제해 점선 텍스처만 바꾼다.
    private static Material DashMaterial(Material spriteMaterial)
    {
        if (dashMaterial != null)
            return dashMaterial;

        Texture2D dash = new Texture2D(2, 1) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
        dash.SetPixels(new[] { Color.white, Color.clear });
        dash.Apply();
        return dashMaterial = new Material(spriteMaterial) { mainTexture = dash };
    }

    // 사거리 원: 흰 점선 테두리 + 옅은 흰 안쪽. 반지름 = 사거리(칸).
    private GameObject RangeCircle(Vector3 center, float radius)
    {
        GameObject root = new GameObject("RangeCircle");
        root.transform.position = center;

        // 안쪽 원 그림은 지름 2칸이라 반지름만큼 키운다.
        GameObject fillObject = new GameObject("Fill", typeof(SpriteRenderer));
        fillObject.transform.SetParent(root.transform, false);
        fillObject.transform.localScale = Vector3.one * radius;
        SpriteRenderer fill = fillObject.GetComponent<SpriteRenderer>();
        fill.sprite = rangeFillSprite;
        fill.color = new Color(1f, 1f, 1f, 0.08f);
        fill.sortingOrder = 15;

        LineRenderer line = new GameObject("Dash", typeof(LineRenderer)).GetComponent<LineRenderer>();
        line.transform.SetParent(root.transform, false);
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = 0.05f;
        line.material = DashMaterial(fill.sharedMaterial);
        line.textureMode = LineTextureMode.Tile;
        line.textureScale = new Vector2(3f, 1f);
        line.sortingOrder = 16;

        const int points = 64;
        line.positionCount = points;
        for (int i = 0; i < points; i++)
        {
            float t = i * Mathf.PI * 2f / points;
            line.SetPosition(i, new Vector3(Mathf.Cos(t), Mathf.Sin(t)) * radius);
        }

        return root;
    }

    // 칸 간격이 1이라 반 칸 안에 있으면 그 칸으로 본다. 없으면 -1.
    public int SlotAt(Vector2 world)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(world, slots[i].position) <= 0.5f)
                return i;
        }

        return -1;
    }

    public bool TrySummon()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return false;

        int slot = FindEmptySlot();

        // 빈 칸이 없으면 소환할 수 없고 마나 결정도 쓰지 않는다.
        if (slot < 0)
        {
            Debug.Log("빈 칸이 없습니다.");
            return false;
        }

        JobData job = RollJob();

        if (job == null || !ManaCrystalManager.Instance.SpendCrystals(summonCost))
            return false;

        Place(job, slot);
        EffectLibrary.Play(EffectLibrary.Instance.summonPortal, slots[slot].position);
        return true;
    }

    public JobUnit Place(JobData job, int slot)
    {
        JobUnit unit = Instantiate(jobUnitPrefab, slots[slot].position, Quaternion.identity);
        unit.Init(job, slot);
        occupants[slot] = unit;
        return unit;
    }

    // 되팔기: 판매 가격만큼 마나 결정을 돌려받고 칸을 비운다 (F-16).
    public void Sell(JobUnit unit)
    {
        ManaCrystalManager.Instance.AddCrystals(unit.JobData.sellPrice);
        EffectLibrary.Play(EffectLibrary.Instance.sell, unit.transform.position);
        Remove(unit);
    }

    public void Remove(JobUnit unit)
    {
        if (unit.SlotIndex >= 0 && occupants[unit.SlotIndex] == unit)
            occupants[unit.SlotIndex] = null;

        Destroy(unit.gameObject);
    }

    // 지금 필드로 만들 수 있는 조합법 목록.
    public List<CombinationTable.Recipe> MakeableRecipes()
    {
        List<JobData> field = new List<JobData>();
        foreach (JobUnit unit in occupants)
        {
            if (unit != null)
                field.Add(unit.JobData);
        }

        return combinations.Makeable(field);
    }

    // 조합 모드 켜기/끄기. 켜고 끌 때마다 고른 재료를 비운다.
    public void SetCombineMode(bool on)
    {
        CombineMode = on;
        Select(-1);
        picks.Clear();
        foreach (GameObject mark in pickMarks)
            Destroy(mark);
        pickMarks.Clear();
    }

    // 조합 모드에서 직업을 누르면 재료로 고르고(최대 4개), 다시 누르면 뺀다.
    private void TogglePick(int slot)
    {
        int index = picks.IndexOf(slot);
        if (index >= 0)
        {
            picks.RemoveAt(index);
            Destroy(pickMarks[index]);
            pickMarks.RemoveAt(index);
            return;
        }

        if (picks.Count >= MaxPicks)
            return;

        // 직업 그림 아래에 깔리도록 낮은 순서로 그린다.
        GameObject mark = EffectLibrary.Loop(EffectLibrary.Instance.combinePick, slots[slot].position, -10);
        picks.Add(slot);
        pickMarks.Add(mark);
    }

    // 고른 재료로 조합한다. 맞는 조합법이 없으면 아무 일도 없다.
    // 재료를 지우고 결과 직업을 첫 번째로 고른 칸에 놓는다. 비용 없음. 성공하면 조합 모드를 끈다.
    public bool CombinePicked()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing || picks.Count < 2)
            return false;

        List<JobData> materials = picks.ConvertAll(slot => occupants[slot].JobData);
        JobData result = combinations.Find(materials);
        if (result == null)
            return false;

        int target = picks[0];
        foreach (int slot in picks)
            Remove(occupants[slot]);

        SetCombineMode(false);
        Place(result, target);
        CombinationTable.MarkFound(result);
        EffectLibrary lib = EffectLibrary.Instance;
        EffectLibrary.Play(result.grade == JobGrade.Hidden ? lib.combineHidden : lib.combineResonance, slots[target].position);
        return true;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == null)
                return i;
        }

        return -1;
    }

    private JobData RollJob()
    {
        // ponytail: 이미지가 없는 직업은 아직 소환되지 않게 뺀다. 직업 이미지가 다 생기면 이 조건을 지운다.
        List<JobData> ready = new List<JobData>();
        foreach (JobData job in jobs)
        {
            if (job.idleSprite != null)
                ready.Add(job);
        }

        if (ready.Count == 0)
        {
            Debug.LogError("소환할 수 있는 직업(이미지 있음)이 없습니다.", this);
            return null;
        }

        // 뽑힌 등급에 나올 직업이 없으면 다시 뽑는다. 남은 등급끼리 확률 비율이 그대로 유지된다.
        for (int attempt = 0; attempt < 100; attempt++)
        {
            JobGrade grade = rates.RollGrade(Random.value);
            List<JobData> pool = ready.FindAll(job => job.grade == grade);

            if (pool.Count > 0)
                return pool[Random.Range(0, pool.Count)];
        }

        return ready[Random.Range(0, ready.Count)];
    }
}
