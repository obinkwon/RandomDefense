using UnityEngine;

// 이펙트 프레임 모음. Assets/Resources/EffectLibrary.asset 하나를 두고 어디서든 EffectLibrary.Instance로 쓴다.
// 원본: ArtSource/effects (2×2 시트를 4프레임으로 자르고 배경을 투명하게 지움). 이름과 용도는 "랜덤직업디펜스 공통 이펙트 시안" 기준.
[CreateAssetMenu(fileName = "EffectLibrary", menuName = "Random Job Defense/Effect Library")]
public class EffectLibrary : ScriptableObject
{
    [Tooltip("summon_portal: 균열 → 차원문 → 빛기둥 → 착지")]
    public Sprite[] summonPortal;
    [Tooltip("combine_resonance: 빛 구슬이 모여 섬광 → 고리")]
    public Sprite[] combineResonance;
    [Tooltip("combine_hidden: 히든 직업 조합. 물음표가 깨진다")]
    public Sprite[] combineHidden;
    [Tooltip("enemy_death: 섬광 → 회색 연기 → 마나 결정")]
    public Sprite[] enemyDeath;
    [Tooltip("sell: 연기 → 펑 → 마나 결정 (판매 버튼이 생기면 쓴다)")]
    public Sprite[] sell;
    [Tooltip("combine_pick: 조합 모드에서 고른 직업 발판 (반복)")]
    public Sprite[] combinePick;
    [Tooltip("boss_appear ② 균열: 보스 등장 때 필드 가운데")]
    public Sprite[] bossCrack;
    [Tooltip("boss_appear ① 경고 띠: 화면 위아래에 이어 붙여 깜빡임 (아직 안 씀)")]
    public Sprite bossBand;

    private static EffectLibrary instance;

    public static EffectLibrary Instance => instance != null ? instance : instance = Resources.Load<EffectLibrary>("EffectLibrary");

    // 한 번 재생하고 사라지는 이펙트. 프레임이 없으면 아무것도 하지 않는다.
    public static void Play(Sprite[] frames, Vector3 position, int sortingOrder = 50)
    {
        if (frames == null || frames.Length == 0)
            return;

        GameObject go = Spawn("Effect", position, sortingOrder);
        FrameAnimator animator = go.GetComponent<FrameAnimator>();
        animator.PlayOnce(frames, null);
        Destroy(go, frames.Length / animator.FramesPerSecond);
    }

    // 계속 반복되는 이펙트. 지울 때는 돌려받은 오브젝트를 Destroy한다.
    public static GameObject Loop(Sprite[] frames, Vector3 position, int sortingOrder)
    {
        GameObject go = Spawn("EffectLoop", position, sortingOrder);
        go.GetComponent<FrameAnimator>().PlayLoop(frames);
        return go;
    }

    private static GameObject Spawn(string name, Vector3 position, int sortingOrder)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(FrameAnimator));
        go.transform.position = position;
        go.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        return go;
    }
}
