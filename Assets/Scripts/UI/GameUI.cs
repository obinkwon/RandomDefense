using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Character Summon")]
    [SerializeField] private Button summonButton;

    private void Start()
    {
        if (summonButton == null)
        {
            Debug.LogError(
                "소환 버튼이 설정되지 않았습니다.",
                this
            );

            return;
        }

        summonButton.onClick.AddListener(
            OnSummonButtonClicked
        );
    }

    private void OnDestroy()
    {
        if (summonButton != null)
        {
            summonButton.onClick.RemoveListener(
                OnSummonButtonClicked
            );
        }
    }

    private void OnSummonButtonClicked()
    {
        SummonManager.Instance.TrySummon();
    }
}