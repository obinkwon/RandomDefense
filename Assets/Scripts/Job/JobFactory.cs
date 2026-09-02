using UnityEngine;

public static class JobFactory
{
    public static JobData GetRandomJob(JobData[] jobs)
    {
        if (jobs == null || jobs.Length == 0)
        {
            Debug.LogError("사용 가능한 직업이 없습니다.");
            return null;
        }

        int randomIndex = Random.Range(0, jobs.Length);

        return jobs[randomIndex];
    }
}