using UnityEngine;

// 투사체 없이 공격 모션과 함께 바로 피해를 주는 즉발 공격 (요구사항 F-21).
[RequireComponent(typeof(JobUnit), typeof(JobTargeting))]
public class JobAttack : MonoBehaviour
{
    private JobUnit unit;
    private JobTargeting targeting;
    private float attackTimer;

    private void Awake()
    {
        unit = GetComponent<JobUnit>();
        targeting = GetComponent<JobTargeting>();
    }

    private void Update()
    {
        JobData job = unit.JobData;

        if (job == null)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        Enemy target = targeting.CurrentTarget;

        if (target == null || !target.IsAlive || !targeting.InRange(target))
            return;

        float damage = DamageCalculator.Calculate(job);

        if (job.areaRadius > 0f)
            HitArea(target.transform.position, job.areaRadius, damage);
        else
            target.TakeDamage(damage);

        // ponytail: 특수공격(skill 열)과 버프 효과는 데이터만 있고 동작은 미구현. 직업별 컴포넌트로 추가 예정.
        unit.Animator.PlayOnce(job.attackFrames, job.idleSprite);
        attackTimer = 1f / job.attackSpeed;
    }

    private static void HitArea(Vector2 center, float radius, float damage)
    {
        // 처치되면 Alive에서 빠지므로 뒤에서부터 돈다.
        for (int i = Enemy.Alive.Count - 1; i >= 0; i--)
        {
            Enemy enemy = Enemy.Alive[i];

            if (Vector2.Distance(center, enemy.transform.position) <= radius)
                enemy.TakeDamage(damage);
        }
    }
}
