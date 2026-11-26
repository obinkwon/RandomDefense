using UnityEngine;

// 필드 칸에 배치된 직업 하나.
public class JobUnit : MonoBehaviour
{
    [SerializeField] private JobData jobData;
    [Tooltip("직업 이미지가 아직 없을 때 쓰는 임시 이미지 (조합 결과 등)")]
    [SerializeField] private Sprite placeholderSprite;

    public JobData JobData => jobData;
    public int SlotIndex { get; private set; } = -1;
    public FrameAnimator Animator { get; private set; }

    public void Init(JobData job, int slotIndex)
    {
        jobData = job;
        SlotIndex = slotIndex;
        name = $"JobUnit_{job.id}";

        // 프리팹에 없으면 붙인다. 직업 스프라이트가 없으면 임시 이미지를 쓴다.
        Animator = TryGetComponent(out FrameAnimator animator) ? animator : gameObject.AddComponent<FrameAnimator>();
        Animator.SetSprite(job.idleSprite != null ? job.idleSprite : placeholderSprite);
    }

    public void MoveToSlot(int slotIndex, Vector3 position)
    {
        SlotIndex = slotIndex;
        transform.position = position;
    }
}
