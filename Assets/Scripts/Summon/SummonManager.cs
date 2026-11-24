using System.Collections.Generic;
using UnityEngine;

// 마나 결정으로 직업을 소환해 빈 칸에 자동 배치한다. 등급을 먼저 뽑고 그 등급 안에서 직업을 균등하게 뽑는다.
public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [SerializeField] private int summonCost = 6;
    [SerializeField] private SummonRates rates;
    [SerializeField] private JobData[] jobs;
    [SerializeField] private JobUnit jobUnitPrefab;
    [Tooltip("필드 가운데의 고정 배치 칸 (기본 6×3)")]
    [SerializeField] private Transform[] slots;

    private JobUnit[] occupants;

    public int SummonCost => summonCost;
    public IReadOnlyList<JobUnit> Occupants => occupants;

    private void Awake()
    {
        Instance = this;
        occupants = new JobUnit[slots.Length];
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
        JobGrade grade = rates.RollGrade(Random.value);
        List<JobData> pool = new List<JobData>();

        foreach (JobData job in jobs)
        {
            if (job.grade == grade)
                pool.Add(job);
        }

        if (pool.Count == 0)
        {
            Debug.LogError($"{grade} 등급 직업이 jobs에 없습니다.", this);
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }
}
