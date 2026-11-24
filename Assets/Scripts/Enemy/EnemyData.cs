using UnityEngine;

// 필드는 밸런스 시트 "스테이지 적" 열을 따른다. 체력은 스테이지 공식으로 정해지므로 StageManager.HpFor 참고.
[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "Random Job Defense/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string enemyName;
    [Tooltip("보스는 한계 수에 포함하지 않는다")]
    public bool isBoss;

    [Header("Stats")]
    [Tooltip("칸/초")]
    public float moveSpeed = 1f;
    public float defense;
    public float magicResist;
    [Tooltip("이동 프레임이 눌림·도약·공중·착지 순서일 때, 도약·공중 동안에만 앞으로 간다 (슬라임)")]
    public bool hopMovement;

    [Header("Reward")]
    public int crystalReward = 2;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite[] moveFrames;

    [Header("Boss")]
    [TextArea] public string gimmick;
}
