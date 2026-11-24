using UnityEngine;

[CreateAssetMenu(
    fileName = "DifficultyData",
    menuName = "Random Job Defense/Difficulty Data"
)]
public class DifficultyData : ScriptableObject
{
    public string displayName;
    public int maxStage = 40;

    [Header("Enemy Multipliers")]
    public float hpMultiplier = 1f;
    public float speedMultiplier = 1f;
}
