using UnityEngine;

[CreateAssetMenu(
    fileName = "JobData",
    menuName = "Random Job Defense/Job Data"
)]
public class JobData : ScriptableObject
{
    [Header("Basic Info")]
    public string jobName;
    public JobType jobType;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackRange = 3f;
    public float attackSpeed = 1f;

    [Header("Ability")]
    public JobAbility ability;
}