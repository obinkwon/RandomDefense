using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 조합표. 히든 조합도 같은 표에 두고, 결과 직업의 grade == Hidden 으로 구분한다.
[CreateAssetMenu(
    fileName = "CombinationTable",
    menuName = "Random Job Defense/Combination Table"
)]
public class CombinationTable : ScriptableObject
{
    [Serializable]
    public class Recipe
    {
        [Tooltip("재료 2~4개. 같은 직업을 여러 번 넣을 수 있다.")]
        public JobData[] materials;
        public JobData result;
    }

    public Recipe[] recipes;

    private Dictionary<string, JobData> lookup;

    // 재료 ID를 정렬해 '|'로 잇는다 (예: archer|mage|mage). 순서와 상관없이 같은 키 (N-03).
    public static string MakeKey(IEnumerable<string> ids)
    {
        return string.Join("|", ids.OrderBy(id => id, StringComparer.Ordinal));
    }

    // 필드 직업으로 만들 수 있는 조합법. 같은 직업이 재료에 여러 번 있으면 필드에도 그만큼 있어야 한다.
    public List<Recipe> Makeable(IEnumerable<JobData> field)
    {
        Dictionary<string, int> have = field.GroupBy(j => j.id).ToDictionary(g => g.Key, g => g.Count());
        return recipes
            .Where(r => r.materials.GroupBy(m => m.id).All(g => have.TryGetValue(g.Key, out int n) && n >= g.Count()))
            .ToList();
    }

    // 히든 직업은 한 번 만들어야 조합표에 드러난다. 찾은 기록은 게임을 껐다 켜도 남는다.
    public static bool IsFound(JobData job)
    {
        return job.grade != JobGrade.Hidden || PlayerPrefs.GetInt("found_" + job.id) == 1;
    }

    public static void MarkFound(JobData job)
    {
        PlayerPrefs.SetInt("found_" + job.id, 1);
    }

    // 맞는 조합법이 없으면 null.
    public JobData Find(IEnumerable<JobData> materials)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, JobData>();

            foreach (Recipe recipe in recipes)
                lookup[MakeKey(recipe.materials.Select(m => m.id))] = recipe.result;
        }

        lookup.TryGetValue(MakeKey(materials.Select(m => m.id)), out JobData result);
        return result;
    }
}
