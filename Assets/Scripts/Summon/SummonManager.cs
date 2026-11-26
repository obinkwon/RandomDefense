using System.Collections.Generic;
using UnityEngine;
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

    private JobUnit[] occupants;
    private int dragFrom = -1;

    public CombinationTable Combinations => combinations;

    public int SummonCost => summonCost;
    public IReadOnlyList<JobUnit> Occupants => occupants;

    private void Awake()
    {
        Instance = this;
        occupants = new JobUnit[slots.Length];
    }

    // 직업을 끌어서 다른 칸으로 옮기거나 자리를 바꾼다 (F-17). 마우스·터치 공통.
    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null || Camera.main == null)
            return;

        Vector3 world = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());
        world.z = 0f;

        if (pointer.press.wasPressedThisFrame && GameManager.Instance.CurrentState == GameState.Playing)
        {
            int slot = SlotAt(world);
            if (slot >= 0 && occupants[slot] != null)
                dragFrom = slot;
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

    // 조합: 재료 직업을 필드에서 지우고 결과 직업을 빈 칸에 놓는다. 비용 없음.
    public bool Combine(CombinationTable.Recipe recipe)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing || !MakeableRecipes().Contains(recipe))
            return false;

        foreach (JobData material in recipe.materials)
        {
            int slot = System.Array.FindIndex(occupants, u => u != null && u.JobData.id == material.id);
            Remove(occupants[slot]);
        }

        dragFrom = -1;
        // 재료를 먼저 지웠으니 빈 칸이 반드시 있다.
        Place(recipe.result, FindEmptySlot());
        CombinationTable.MarkFound(recipe.result);
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
