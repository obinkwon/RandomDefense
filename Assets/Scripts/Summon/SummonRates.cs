using UnityEngine;

// 소환 등급 확률. 소환으로는 일반·고급·희귀만 나온다 (요구사항 F-20).
[CreateAssetMenu(
    fileName = "SummonRates",
    menuName = "Random Job Defense/Summon Rates"
)]
public class SummonRates : ScriptableObject
{
    public float normal = 80f;
    public float advanced = 19f;
    public float rare = 1f;

    // roll: 0 이상 1 미만.
    public JobGrade RollGrade(float roll)
    {
        float r = roll * (normal + advanced + rare);

        if (r < normal)
            return JobGrade.Normal;

        if (r < normal + advanced)
            return JobGrade.Advanced;

        return JobGrade.Rare;
    }
}
