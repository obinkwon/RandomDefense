using UnityEngine;

// 스프라이트 프레임을 바꿔 그리는 간단한 애니메이션. 반복 재생하거나 한 번 재생 후 대기 스프라이트로 돌아간다.
[RequireComponent(typeof(SpriteRenderer))]
public class FrameAnimator : MonoBehaviour
{
    [SerializeField] private float framesPerSecond = 10f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private Sprite restSprite;
    private bool loop;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        frames = null;
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    public void PlayLoop(Sprite[] newFrames)
    {
        Play(newFrames, true, null);
    }

    public void PlayOnce(Sprite[] newFrames, Sprite rest)
    {
        Play(newFrames, false, rest);
    }

    private void Play(Sprite[] newFrames, bool repeat, Sprite rest)
    {
        if (newFrames == null || newFrames.Length == 0)
            return;

        frames = newFrames;
        loop = repeat;
        restSprite = rest;
        timer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null)
            return;

        timer += Time.deltaTime;
        int index = (int)(timer * framesPerSecond);

        if (index >= frames.Length)
        {
            if (loop)
            {
                timer %= frames.Length / framesPerSecond;
                index %= frames.Length;
            }
            else
            {
                SetSprite(restSprite);
                return;
            }
        }

        spriteRenderer.sprite = frames[index];
    }
}
