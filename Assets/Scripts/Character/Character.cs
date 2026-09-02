using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Job")]
    [SerializeField] private JobData jobData;

    [Header("Character Stats")]
    [SerializeField] private CharacterStats stats;

    private float currentHp;

    public JobData JobData => jobData;
    public CharacterStats Stats => stats;
    public float CurrentHp => currentHp;

    private void Start()
    {
        if (jobData == null)
        {
            AssignRandomJob();
        }

        ApplyJobData();

        currentHp = stats.maxHp;

        Debug.Log(
            $"캐릭터 생성 - 직업: {jobData.jobName}, HP: {currentHp}"
        );
    }

    private void AssignRandomJob()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다.", this);
            return;
        }

        jobData = GameManager.Instance.GetRandomJob();

        if (jobData == null)
        {
            Debug.LogError("랜덤 직업을 가져오지 못했습니다.", this);
        }
    }

    private void ApplyJobData()
    {
        if (jobData == null)
        {
            Debug.LogError("JobData가 설정되지 않았습니다.", this);
            return;
        }

        stats.attackDamage = jobData.attackDamage;
        stats.attackRange = jobData.attackRange;
        stats.attackSpeed = jobData.attackSpeed;
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

    private void Die()
    {
        Debug.Log("캐릭터 사망");

        Destroy(gameObject);
    }

    public void SetJob(JobData newJob)
    {
        if (newJob == null)
            return;

        jobData = newJob;

        ApplyJobData();

        currentHp = stats.maxHp;

        Debug.Log(
            $"직업 변경: {jobData.jobName}"
        );
    }
}