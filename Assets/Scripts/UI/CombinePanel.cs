using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 조합 목록 / 조합표 창. 조합 목록은 지금 필드로 만들 수 있는 조합만, 조합표(showAll)는 전체 조합을 보여준다.
// 만들 수 있는 줄은 밝고 누르면 바로 조합한다. 아직 못 찾은 히든 조합은 결과(조합표에서는 재료도)를 ???로 가린다.
public class CombinePanel : MonoBehaviour
{
    [Tooltip("켜면 조합표: 만들 수 없는 조합도 전부 보여준다.")]
    [SerializeField] private bool showAll;
    [SerializeField] private Transform list;
    [Tooltip("한 줄 견본. 꺼 둔 채로 두고 복제해 쓴다.")]
    [SerializeField] private Button rowTemplate;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private Button closeButton;

    private static readonly Color DimText = new Color(0.6f, 0.6f, 0.6f);

    private void Awake()
    {
        rowTemplate.gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        foreach (Transform child in list)
        {
            if (child != rowTemplate.transform && child != emptyText.transform.parent)
                Destroy(child.gameObject);
        }

        List<CombinationTable.Recipe> makeable = SummonManager.Instance.MakeableRecipes();
        IEnumerable<CombinationTable.Recipe> shown = showAll ? SummonManager.Instance.Combinations.recipes : makeable;
        emptyText.transform.parent.gameObject.SetActive(!shown.Any());

        foreach (CombinationTable.Recipe recipe in shown)
        {
            bool canMake = makeable.Contains(recipe);
            bool found = CombinationTable.IsFound(recipe.result);

            Button row = Instantiate(rowTemplate, list);
            row.gameObject.SetActive(true);
            row.interactable = canMake;

            TMP_Text text = row.GetComponentInChildren<TMP_Text>();
            string materials = found || canMake ? string.Join(" + ", recipe.materials.Select(m => m.jobName)) : "???";
            text.text = $"{materials}  →  {(found ? recipe.result.jobName : "???")}";
            text.color = canMake ? Color.white : DimText;

            row.onClick.AddListener(() =>
            {
                SummonManager.Instance.Combine(recipe);
                Refresh();
            });
        }
    }
}
