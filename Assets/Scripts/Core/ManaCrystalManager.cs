using UnityEngine;

// 소환 재화 "마나 결정"을 관리한다.
public class ManaCrystalManager : MonoBehaviour
{
    public static ManaCrystalManager Instance { get; private set; }

    [Tooltip("게임 시작 시 마나 결정 (소환 3번 분량)")]
    [SerializeField] private int currentCrystals = 18;

    public int CurrentCrystals => currentCrystals;

    private void Awake()
    {
        Instance = this;
    }

    public void AddCrystals(int amount)
    {
        if (amount <= 0)
            return;

        currentCrystals += amount;
    }

    public bool SpendCrystals(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentCrystals < amount)
        {
            Debug.Log("마나 결정이 부족합니다.");
            return false;
        }

        currentCrystals -= amount;
        return true;
    }
}
