using UnityEngine;

// 필드 칸에 배치된 직업 하나.
public class JobUnit : MonoBehaviour
{
    [SerializeField] private JobData jobData;

    public JobData JobData => jobData;
    public int SlotIndex { get; private set; } = -1;

    public void Init(JobData job, int slotIndex)
    {
        jobData = job;
        SlotIndex = slotIndex;
        name = $"JobUnit_{job.id}";
    }

    public void MoveToSlot(int slotIndex, Vector3 position)
    {
        SlotIndex = slotIndex;
        transform.position = position;
    }
}
