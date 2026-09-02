using UnityEngine;

public static class DamageCalculator
{
    public static float Calculate(Character character)
    {
        if (character == null)
            return 0f;

        float damage = character.Stats.attackDamage;

        if (character.JobData == null)
            return damage;

        switch (character.JobData.ability)
        {
            case JobAbility.CriticalHit:
                damage *= 1.5f;
                break;

            case JobAbility.SplashDamage:
                break;

            case JobAbility.Slow:
                break;

            case JobAbility.Heal:
                break;
        }

        return damage;
    }
}