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

    public const int MaxPicks = 4;

    private JobUnit[] occupants;
    private int dragFrom = -1;
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

    // 직업을 끌어서 다른 칸으로 옮기거나 자리를 바꾼다 (F-17). 조합 모드에서는 누른 직업을 재료로 고른다. 마우스·터치 공통.
    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null || Camera.main == null)
            return;

        Vector3 world = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());
        world.z = 0f;

        // UI(조합표 창과 뒤 가림막 포함) 위를 누르면 필드는 반응하지 않는다.
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (pointer.press.wasPressedThisFrame && !overUi && GameManager.Instance.CurrentState == GameState.Playing)
        {
            int slot = SlotAt(world);
            if (slot >= 0 && occupants[slot] != null)
            {
                if (CombineMode)
                    TogglePick(slot);
                else
                    dragFrom = slot;
            }
        }

        if (dragFrom < 0)
            return;

        JobUnit unit = occupants[dragFrom];
        if (unit == null)
        {
            dragFrom = -1;
            return;
        }

        if (pointer.press.isPressed)
        {
            unit.transform.position = world;
            return;
        }

        int to = SlotAt(world);
        if (to >= 0 && to != dragFrom)
            Swap(dragFrom, to);
        else
            unit.transform.position = slots[dragFrom].position;

        dragFrom = -1;
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
        dragFrom = -1;
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
        if (GameManager.Instance.CurrentState != GameState.Playing || picks.Count == 0)
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

    // 끌어서 칸 옮기기·자리 바꾸기 (요구사항 F-17).
    public void Swap(int from, int to)
    {
        JobUnit a = occupants[from];
        JobUnit b = occupants[to];
        occupants[from] = b;
        occupants[to] = a;

        if (a != null) a.MoveToSlot(to, slots[to].position);
        if (b != null) b.MoveToSlot(from, slots[from].position);
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
