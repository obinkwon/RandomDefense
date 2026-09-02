using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "Random Job Defense/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;

    [Header("Stats")]
    public float maxHp = 100f;
    public float moveSpeed = 1f;

    [Header("Reward")]
    public int goldReward = 10;
}