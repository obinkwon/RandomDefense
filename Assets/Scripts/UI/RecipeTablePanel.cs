using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 조합표 창 (공통 에셋 시안 "조합표 팝업"). 왼쪽 세로 등급 탭, 오른쪽 2열 조합 카드.
// 카드: 결과 얼굴 < 재료 + 재료. 재료 얼굴을 누르면 그 직업이 들어가는 조합만 보여준다.
// 아직 못 찾은 히든 조합은 결과·재료를 ???로 가린다. 탭·카드는 열 때마다 코드로 만든다.
public class RecipeTablePanel : MonoBehaviour
{
    [SerializeField] private RectTransform tabs;
    [Tooltip("카드가 들어가는 2열 GridLayoutGroup (ScrollRect의 content)")]
    [SerializeField] private GridLayoutGroup list;
    [SerializeField] private TMP_Text emptyText;
    [Tooltip("재료로 걸러 볼 때 보이는 '재료: 이름' 버튼. 누르면 거르기를 푼다.")]
    [SerializeField] private Button filterChip;
    [SerializeField] private Button closeButton;
    [Tooltip("창 뒤를 어둡게 가리는 막. 누르면 창이 닫힌다.")]
    [SerializeField] private Button overlay;

    [Header("Art")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Sprite cardFrame;
    [SerializeField] private Sprite lessThan;
    [SerializeField] private Sprite plus;
    [SerializeField] private Sprite question;

    private static readonly JobGrade[] TabGrades = { JobGrade.Advanced, JobGrade.Rare, JobGrade.Hero, JobGrade.Hidden };
    private static readonly string[] TabLabels = { "고급", "희귀", "영웅", "히든" };
    private static readonly Color TextColor = new Color32(0xf3, 0xe6, 0xd0, 0xff);
    private static readonly Color Border = new Color32(0x1d, 0x14, 0x0d, 0xff);
    private static readonly Color Locked = new Color32(0x1a, 0x1a, 0x1f, 0xff);
    private static readonly Color Gold = new Color32(0xf2, 0xc2, 0x30, 0xff);

    private JobGrade tab = JobGrade.Advanced;
    private JobData filter;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        overlay.onClick.AddListener(Close);
        filterChip.onClick.AddListener(() => SetFilter(null));
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        filter = null;
        gameObject.SetActive(true);
        overlay.gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        overlay.gameObject.SetActive(false);
    }

    public static Color GradeColor(JobGrade grade)
    {
        switch (grade)
        {
            case JobGrade.Advanced: return new Color32(0x3f, 0x9a, 0x3a, 0xff);
            case JobGrade.Rare: return new Color32(0x3a, 0x6f, 0xd0, 0xff);
            case JobGrade.Hero: return new Color32(0x8b, 0x4f, 0xd0, 0xff);
            case JobGrade.Hidden: return new Color32(0x6b, 0x6b, 0x78, 0xff);
            default: return new Color32(0x8a, 0x8f, 0x86, 0xff);
        }
    }

    private void SetFilter(JobData job)
    {
        filter = job;
        Refresh();
    }

    private void Refresh()
    {
        Clear(tabs);
        Clear(list.transform);

        filterChip.gameObject.SetActive(filter != null);
        if (filter != null)
            filterChip.GetComponentInChildren<TMP_Text>().text = $"재료: {filter.jobName} · 해제";

        for (int i = 0; i < TabGrades.Length; i++)
            BuildTab(TabGrades[i], TabLabels[i]);

        // 2열: 칸 너비를 목록 너비에 맞춘다.
        float width = ((RectTransform)list.transform.parent).rect.width;
        list.cellSize = new Vector2((width - list.spacing.x) / 2f, list.cellSize.y);

        // 거르기 중이면 등급과 상관없이, 이미 찾은 조합 중 그 재료가 들어가는 것만.
        IEnumerable<CombinationTable.Recipe> recipes = SummonManager.Instance.Combinations.recipes;
        recipes = filter != null
            ? recipes.Where(r => CombinationTable.IsFound(r.result) && r.materials.Any(m => m.id == filter.id))
            : recipes.Where(r => r.result.grade == tab);

        int count = 0;
        foreach (CombinationTable.Recipe recipe in recipes)
        {
            BuildCard(recipe);
            count++;
        }

        emptyText.gameObject.SetActive(count == 0);
    }

    private void BuildTab(JobGrade grade, string label)
    {
        bool on = filter == null && tab == grade;

        RectTransform rt = NewRect("Tab_" + label, tabs);
        rt.sizeDelta = new Vector2(0, 90);
        Image frame = Frame(rt, cardFrame, 3f);
        frame.color = on ? Color.white : new Color(0.75f, 0.75f, 0.75f);
        if (on)
        {
            Outline glow = frame.gameObject.AddComponent<Outline>();
            glow.effectColor = Gold;
            glow.effectDistance = new Vector2(5, -5);
        }

        RectTransform tint = NewRect("Tint", rt);
        Stretch(tint, 27, 29, 27, 24);
        Color c = GradeColor(grade);
        c.a = on ? 0.7f : 0.18f;
        tint.gameObject.AddComponent<Image>().color = c;
        tint.GetComponent<Image>().raycastTarget = false;

        Text(rt, label, 26);

        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = frame;
        button.onClick.AddListener(() =>
        {
            tab = grade;
            SetFilter(null);
        });
    }

    private void BuildCard(CombinationTable.Recipe recipe)
    {
        bool found = CombinationTable.IsFound(recipe.result);

        RectTransform card = NewRect("Card", list.transform);
        Frame(card, cardFrame, 3f);

        RectTransform strip = NewRect("Strip", card);
        strip.anchorMin = new Vector2(0, 0);
        strip.anchorMax = new Vector2(0, 1);
        strip.pivot = new Vector2(0, 0.5f);
        strip.offsetMin = new Vector2(26, 26);
        strip.offsetMax = new Vector2(36, -22);
        strip.gameObject.AddComponent<Image>().color = GradeColor(recipe.result.grade);

        RectTransform row = NewRect("Row", card);
        Stretch(row, 48, 14, 6, 14);
        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Face(row, found ? recipe.result : null, 70, null);
        Symbol(row, lessThan, 42);

        for (int i = 0; i < recipe.materials.Length; i++)
        {
            if (i > 0)
                Symbol(row, plus, 29);

            JobData material = recipe.materials[i];
            Face(row, found ? material : null, 64, found ? () => SetFilter(material) : (UnityEngine.Events.UnityAction)null);
        }
    }

    // 직업 얼굴: 등급 색 네모 + 이름 첫 글자, 아래에 이름. job이 null이면 ??? 칸.
    // ponytail: 직업 얼굴 그림이 없어 첫 글자로 대신한다. 얼굴 그림이 생기면 JobData.idleSprite를 넣는다.
    private void Face(RectTransform parent, JobData job, float size, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = NewRect("Face", parent);
        rt.sizeDelta = new Vector2(size + 16, size + 34);

        RectTransform box = NewRect("Box", rt);
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 1);
        box.pivot = new Vector2(0.5f, 1);
        box.sizeDelta = new Vector2(size, size);
        box.gameObject.AddComponent<Image>().color = Border;

        RectTransform inner = NewRect("Inner", box);
        Stretch(inner, 5, 5, 5, 5);
        Image fill = inner.gameObject.AddComponent<Image>();
        fill.color = job != null ? GradeColor(job.grade) : Locked;

        if (job != null)
        {
            Text(inner, job.jobName.Substring(0, 1), size * 0.42f).color = Color.white;
        }
        else
        {
            RectTransform q = NewRect("Question", inner);
            Stretch(q, size * 0.2f, size * 0.2f, size * 0.2f, size * 0.2f);
            Image qi = q.gameObject.AddComponent<Image>();
            qi.sprite = question;
            qi.preserveAspect = true;
        }

        RectTransform name = NewRect("Name", rt);
        name.anchorMin = new Vector2(0, 0);
        name.anchorMax = new Vector2(1, 0);
        name.pivot = new Vector2(0.5f, 0);
        name.sizeDelta = new Vector2(40, 30);
        TMP_Text nameText = Text(name, job != null ? job.jobName : "???", size > 66 ? 19 : 18);
        nameText.enableWordWrapping = false;
        nameText.overflowMode = TextOverflowModes.Overflow;

        if (onClick != null)
        {
            Button button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;
            button.onClick.AddListener(onClick);
        }
    }

    private void Symbol(RectTransform parent, Sprite sprite, float size)
    {
        RectTransform rt = NewRect("Symbol", parent);
        rt.sizeDelta = new Vector2(size, size);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    // 9칸 나눔(Sliced) 액자. multiplier가 클수록 모서리가 작게 그려진다.
    private static Image Frame(RectTransform parent, Sprite sprite, float multiplier)
    {
        RectTransform rt = NewRect("Frame", parent);
        Stretch(rt, 0, 0, 0, 0);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = multiplier;
        return image;
    }

    private TMP_Text Text(RectTransform parent, string value, float size)
    {
        RectTransform rt = NewRect("Text", parent);
        Stretch(rt, 0, 0, 0, 0);
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = TextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // 부모를 채우되 왼쪽·아래·오른쪽·위를 그만큼 띄운다.
    private static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static void Clear(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}
