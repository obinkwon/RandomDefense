using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackRange = 3f;
    public float attackSpeed = 1f;

    [Header("Health")]
    public float maxHp = 100f;
}