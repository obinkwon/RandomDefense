using UnityEngine;

[RequireComponent(typeof(JobUnit))]
public class JobTargeting : MonoBehaviour
{
    private JobUnit unit;
    private Enemy currentTarget;

    public Enemy CurrentTarget => currentTarget;

    private void Awake()
    {
        unit = GetComponent<JobUnit>();
    }

    private void Update()
    {
        // 아군 버프 직업은 적을 노리지 않는다.
        if (unit.JobData == null || unit.JobData.targetRule == TargetRule.AllyBuff)
        {
            currentTarget = null;
            return;
        }

        // 공격 중인 적이 사거리 안에 살아 있으면 계속 그 적을 공격한다.
        if (currentTarget != null && currentTarget.IsAlive && InRange(currentTarget))
            return;

        currentTarget = FindTarget();
    }

    public bool InRange(Enemy enemy)
    {
        return Vector2.Distance(transform.position, enemy.transform.position)
            <= unit.JobData.attackRange;
    }

    private Enemy FindTarget()
    {
        Enemy best = null;

        foreach (Enemy enemy in Enemy.Alive)
        {
            if (!InRange(enemy))
                continue;

            if (best == null || IsBetter(enemy, best))
                best = enemy;
        }

        return best;
    }

    private bool IsBetter(Enemy a, Enemy b)
    {
        switch (unit.JobData.targetRule)
        {
            case TargetRule.LeastHp:
                return a.CurrentHp < b.CurrentHp;
            case TargetRule.NotSlowed:
                return !a.IsSlowed && b.IsSlowed;
            case TargetRule.BossFirst:
                if (a.Data.isBoss != b.Data.isBoss)
                    return a.Data.isBoss;
                return a.CurrentHp > b.CurrentHp;
            default:
                return a.CurrentHp > b.CurrentHp;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (unit != null && unit.JobData != null)
            Gizmos.DrawWireSphere(transform.position, unit.JobData.attackRange);
    }
}
