using UnityEngine;

public static class DamageCalculator
{
    // ponytail: 방어력·마법 저항·관통은 아직 미적용. 적 방어 공식 확정 후 추가.
    public static float Calculate(JobData job)
    {
        if (job == null)
            return 0f;

        float damage = job.attackDamage + job.spellPower;

        if (Random.value < job.critChance)
            damage *= job.critDamage;

        return damage;
    }
}
