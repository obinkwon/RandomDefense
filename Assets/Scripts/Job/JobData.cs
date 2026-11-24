using UnityEngine;

public enum JobGrade
{
    Normal,
    Advanced,
    Rare,
    Hero,
    Hidden
}

public enum JobRole
{
    SingleTarget,
    MultiTarget,
    Area,
    Slow,
    Buff
}

public enum TargetRule
{
    MostHp,
    LeastHp,
    NotSlowed,
    AllyBuff,
    BossFirst
}

public enum SpecialTrigger
{
    Always,
    EveryNthAttack,
    EveryNSeconds,
    Chance
}

// 필드는 밸런스 시트 "직업 스탯" 열 순서를 따른다. 거리 단위 1칸 = 1 유닛.
[CreateAssetMenu(
    fileName = "JobData",
    menuName = "Random Job Defense/Job Data"
)]
public class JobData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("조합 키에 쓰는 ID (예: archer)")]
    public string id;
    public string jobName;
    public JobGrade grade;
    public JobRole role;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float spellPower;
    public float attackRange = 3f;
    public float attackSpeed = 1f;
    public TargetRule targetRule;
    [Tooltip("0이면 단일 대상")]
    public float areaRadius;
    public int targetCount = 1;
    public int sellPrice;

    [Header("Critical / Penetration")]
    [Range(0f, 1f)] public float critChance;
    public float critDamage = 1.5f;
    public float armorPenetration;
    public float magicPenetration;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite[] attackFrames;

    [Header("Special Attack")]
    public string skillName;
    [TextArea] public string skillDescription;
    public SpecialTrigger trigger;
    public float triggerValue;
    public float specialMultiplier;
    public float specialRange;
    public float effectValue;
    public float effectDuration;
}
