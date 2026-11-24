using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("Gold")]
    [SerializeField] private int currentGold = 100;
    [SerializeField] private int characterCost = 50;

    public int CurrentGold => currentGold;
    public int CharacterCost => characterCost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        Debug.Log(
            $"골드 획득: +{amount} / 현재 골드: {currentGold}"
        );
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentGold < amount)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        currentGold -= amount;

        Debug.Log(
            $"골드 사용: -{amount} / 현재 골드: {currentGold}"
        );

        return true;
    }

    public bool SpendCharacterCost()
    {
        return SpendGold(characterCost);
    }
}