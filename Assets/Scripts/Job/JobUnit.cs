using UnityEngine;

// 필드 칸에 배치된 직업 하나.
public class JobUnit : MonoBehaviour
{
    [SerializeField] private JobData jobData;

    public JobData JobData => jobData;
    public int SlotIndex { get; private set; } = -1;
    public FrameAnimator Animator { get; private set; }

    public void Init(JobData job, int slotIndex)
    {
        jobData = job;
        SlotIndex = slotIndex;
        name = $"JobUnit_{job.id}";

        // 프리팹에 없으면 붙인다. 직업 스프라이트가 없으면 프리팹 기본 이미지를 쓴다.
        Animator = TryGetComponent(out FrameAnimator animator) ? animator : gameObject.AddComponent<FrameAnimator>();
        Animator.SetSprite(job.idleSprite);
    }

    public void MoveToSlot(int slotIndex, Vector3 position)
    {
        SlotIndex = slotIndex;
        transform.position = position;
    }
}
