using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Character character;
    [SerializeField] private CharacterTarget targetSystem;

    private float attackTimer;

    private void Start()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }

        if (targetSystem == null)
        {
            targetSystem = GetComponent<CharacterTarget>();
        }
    }

    private void Update()
    {
        if (character == null || targetSystem == null)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        Attack();
    }

    private void Attack()
    {
        Enemy target = targetSystem.CurrentTarget;

        if (target == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            target.transform.position
        );

        if (distance > character.Stats.attackRange)
            return;

        float damage = DamageCalculator.Calculate(character);

        target.TakeDamage(damage);

        ApplyJobAbility(target);

        attackTimer = 1f / character.Stats.attackSpeed;

        Debug.Log(
            $"공격! {target.name}에게 {damage} 데미지"
        );
    }

    private void ApplyJobAbility(Enemy target)
    {
        if (character.JobData == null)
            return;

        switch (character.JobData.ability)
        {
            case JobAbility.Slow:
                target.ApplyStatusEffect(
                    new StatusEffect(
                        StatusEffectType.Slow,
                        3f,
                        0.5f
                    )
                );
                break;

            case JobAbility.SplashDamage:
                break;

            case JobAbility.Heal:
                break;

            case JobAbility.CriticalHit:
                break;
        }
    }
}