using System.Collections.Generic;
using UnityEngine;

// 필드 둘레의 순환 경로(웨이포인트 루프)를 계속 도는 적.
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> Alive = new List<Enemy>();

    [SerializeField] private EnemyData enemyData;

    private Transform[] waypoints;
    private int nextWaypoint;
    private float maxHp;
    private float currentHp;
    private float moveSpeed;

    // 둔화는 중첩되지 않고 가장 강한 것 하나만 적용한다.
    private float slowRate = 1f;
    private float slowTimer;

    public EnemyData Data => enemyData;
    public float CurrentHp => currentHp;
    public bool IsAlive => currentHp > 0f;
    public bool IsSlowed => slowTimer > 0f;

    private SpriteRenderer spriteRenderer;
    private FrameAnimator animator;

    public void Init(EnemyData data, Transform[] path, float hp, float speedMultiplier)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = false; // 처음엔 오른쪽을 본다
        enemyData = data;
        name = data.id;
        waypoints = path;
        nextWaypoint = path.Length > 1 ? 1 : 0;
        maxHp = hp;
        currentHp = maxHp;
        moveSpeed = enemyData.moveSpeed * speedMultiplier;
        Alive.Add(this);

        animator = TryGetComponent(out FrameAnimator a) ? a : gameObject.AddComponent<FrameAnimator>();
        animator.SetSprite(data.idleSprite);
        animator.PlayLoop(data.moveFrames);
    }

    private void OnDestroy()
    {
        Alive.Remove(this);
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                slowRate = 1f;
        }

        Vector3 target = waypoints[nextWaypoint].position;

        // 원본 그림은 오른쪽을 본다. 왼쪽으로 갈 때만 뒤집고, 위아래로 갈 때는 직전 방향 유지.
        float dx = target.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            spriteRenderer.flipX = dx < 0f;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * slowRate * HopFactor() * Time.deltaTime);

        if (transform.position == target)
            nextWaypoint = (nextWaypoint + 1) % waypoints.Length;
    }

    // 뛰는 적은 도약·공중(1, 2번 프레임)에만 2배로 가고 눌림·착지 때는 멈춘다. 평균 속도는 그대로.
    private float HopFactor()
    {
        if (!enemyData.hopMovement || animator.FrameIndex < 0)
            return 1f;

        return animator.FrameIndex == 1 || animator.FrameIndex == 2 ? 2f : 0f;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || !IsAlive)
            return;

        currentHp -= damage;

        if (currentHp <= 0f)
            Die();
    }

    // rate: 이동 속도 배율 (0.5 = 50% 속도).
    public void ApplySlow(float rate, float duration)
    {
        if (slowTimer > 0f && rate > slowRate)
            return;

        if (rate < slowRate || duration > slowTimer)
            slowTimer = duration;

        slowRate = rate;
    }

    private void Die()
    {
        if (ManaCrystalManager.Instance != null)
            ManaCrystalManager.Instance.AddCrystals(enemyData.crystalReward);

        EffectLibrary.Play(EffectLibrary.Instance.enemyDeath, transform.position);

        Alive.Remove(this);
        Destroy(gameObject);
    }
}
