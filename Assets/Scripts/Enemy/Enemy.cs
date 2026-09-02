using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Movement")]
    [SerializeField] private Transform target;

    [Header("Target Settings")]
    [SerializeField] private float targetDistance = 0.1f;

    private float currentHp;
    private float currentMoveSpeed;

    private StatusEffect activeStatusEffect;

    public EnemyData Data => enemyData;
    public float CurrentHp => currentHp;

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData가 설정되지 않았습니다.", this);
            return;
        }

        currentHp = enemyData.maxHp;
        currentMoveSpeed = enemyData.moveSpeed;

        Debug.Log(
            $"{enemyData.enemyName} 생성 - HP: {currentHp}"
        );
    }

    private void Update()
    {
        if (target == null)
            return;

        UpdateStatusEffect();
        MoveToTarget();
    }

    private void UpdateStatusEffect()
    {
        if (activeStatusEffect == null)
        {
            currentMoveSpeed = enemyData.moveSpeed;
            return;
        }

        activeStatusEffect.duration -= Time.deltaTime;

        if (activeStatusEffect.duration <= 0f)
        {
            activeStatusEffect = null;
            currentMoveSpeed = enemyData.moveSpeed;

            Debug.Log($"{enemyData.enemyName} 상태이상 종료");
            return;
        }

        ApplyStatusEffect();
    }

    private void ApplyStatusEffect()
    {
        if (activeStatusEffect == null)
            return;

        switch (activeStatusEffect.type)
        {
            case StatusEffectType.Slow:
                currentMoveSpeed =
                    enemyData.moveSpeed * activeStatusEffect.value;
                break;

            case StatusEffectType.Poison:
                break;

            case StatusEffectType.Stun:
                currentMoveSpeed = 0f;
                break;
        }
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            target.position
        ) <= targetDistance)
        {
            ReachTarget();
        }
    }

    private void ReachTarget()
    {
        Debug.Log(
            $"{enemyData.enemyName}이(가) 목표 지점에 도착했습니다."
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        currentHp -= damage;

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null)
            return;

        activeStatusEffect = effect;

        Debug.Log(
            $"{enemyData.enemyName}에게 {effect.type} 적용"
        );
    }

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} 처치");

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(
                enemyData.goldReward
            );
        }

        Destroy(gameObject);
    }
}