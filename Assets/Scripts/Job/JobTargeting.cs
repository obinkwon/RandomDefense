using UnityEngine;

public class CharacterTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private float searchRange = 5f;

    private Enemy currentTarget;

    public Enemy CurrentTarget => currentTarget;

    private void Update()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            searchRange
        );

        Enemy nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        currentTarget = nearestEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            searchRange
        );
    }
}