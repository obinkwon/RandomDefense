using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 열린 씬의 Canvas에 HUD를 깐다. 위쪽 한 줄: 카운터 / 오른쪽 위: 일시정지·배속·설정 / 아래쪽 한 줄: 소환·조합·조합표.
// 1920×1080 기준, 맵 한 칸 = 120px이라 위아래 한 줄 안에 들어가게 높이를 맞춘다.
public static class HudSetup
{
    private const string PrefabDir = "Assets/Prefabs/UI/";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Regular SDF.asset";

    [MenuItem("RandomDefense/HUD 배치")]
    private static void Build()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameUI gameUI = Object.FindFirstObjectByType<GameUI>();

        if (canvas == null || gameUI == null)
        {
            Debug.LogError("InGame 씬을 연 상태에서 실행하세요.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Counter_Time.prefab") == null)
            UiPrefabBuilder.Build();

        // 화면 크기와 상관없이 1920×1080 기준으로 맞춘다.
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        Undo.RecordObject(scaler, "HUD 배치");
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 예전 임시 버튼과 이전 HUD 제거
        foreach (string old in new[] { "SummonButton", "HUD" })
        {
            Transform t = canvas.transform.Find(old);
            if (t != null)
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        RectTransform hud = NewRect("HUD", canvas.transform);
        Stretch(hud);

        // 위쪽 한 줄: 카운터 (왼쪽부터)
        float x = 20f;
        TMP_Text crystal = Counter("Counter_Crystal", hud, ref x);
        TMP_Text life = Counter("Counter_Life", hud, ref x);
        TMP_Text stage = Counter("Counter_Stage", hud, ref x);
        TMP_Text time = Counter("Counter_Time", hud, ref x);
        TMP_Text enemy = Counter("Counter_Enemy", hud, ref x);

        // 오른쪽 위: 설정, 배속, 일시정지 (오른쪽부터)
        Button settings = Place("Button_Settings", hud, new Vector2(1, 1), new Vector2(-20, -10), 100);
        Button fast = Place("Button_Fast", hud, new Vector2(1, 1), new Vector2(-130, -10), 100);
        Button pause = Place("Button_Pause", hud, new Vector2(1, 1), new Vector2(-240, -10), 100);

        // 아래쪽 한 줄: 조합표 · 소환 · 조합
        Button summon = Place("Button_Summon", hud, new Vector2(0.5f, 0), new Vector2(0, 8), 110);
        Button combine = TextButton("Button_Combine", "조합", hud, new Vector2(230, 14));
        Button table = TextButton("Button_CombineTable", "조합표", hud, new Vector2(-230, 14));

        // 다른 HUD 위에 그려지도록 마지막에 만든다.
        CombinePanel combinePanel = BuildCombinePanel(hud, "조합", false);
        CombinePanel tablePanel = BuildCombinePanel(hud, "조합표", true);

        SerializedObject so = new SerializedObject(gameUI);
        so.FindProperty("crystalText").objectReferenceValue = crystal;
        so.FindProperty("lifeText").objectReferenceValue = life;
        so.FindProperty("stageText").objectReferenceValue = stage;
        so.FindProperty("timeText").objectReferenceValue = time;
        so.FindProperty("enemyText").objectReferenceValue = enemy;
        so.FindProperty("summonButton").objectReferenceValue = summon;
        so.FindProperty("pauseButton").objectReferenceValue = pause;
        so.FindProperty("fastButton").objectReferenceValue = fast;
        so.FindProperty("combineButton").objectReferenceValue = combine;
        so.FindProperty("combinePanel").objectReferenceValue = combinePanel;
        so.FindProperty("tableButton").objectReferenceValue = table;
        so.FindProperty("tablePanel").objectReferenceValue = tablePanel;
        so.FindProperty("pauseSprite").objectReferenceValue = LoadSprite("btn_pause");
        so.FindProperty("playSprite").objectReferenceValue = LoadSprite("btn_play");
        so.FindProperty("fastSprite").objectReferenceValue = LoadSprite("btn_fast");
        so.FindProperty("normalSprite").objectReferenceValue = LoadSprite("btn_normal");
        so.ApplyModifiedProperties();

        // 조합 목록이 쓰는 조합표 연결 (씬 파일을 직접 고치면 열린 씬 저장 때 덮어써진다)
        SummonManager summonManager = Object.FindFirstObjectByType<SummonManager>();
        string[] tables = AssetDatabase.FindAssets("t:CombinationTable");
        if (summonManager == null || tables.Length == 0)
        {
            Debug.LogError("SummonManager 또는 CombinationTable 에셋을 찾지 못했습니다.");
        }
        else
        {
            SerializedObject summonSo = new SerializedObject(summonManager);
            summonSo.FindProperty("combinations").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CombinationTable>(AssetDatabase.GUIDToAssetPath(tables[0]));
            summonSo.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("HUD 배치 완료. 설정 버튼은 아직 기능이 연결되지 않았습니다. Ctrl+S로 저장하세요.");
    }

    // 화면 가운데 조합 창: 제목, 닫기 버튼, 세로 스크롤 목록(줄 견본 + 빈 목록 안내).
    // showAll이면 조합표(전체 조합), 아니면 조합 목록(지금 만들 수 있는 조합).
    private static CombinePanel BuildCombinePanel(RectTransform hud, string titleText, bool showAll)
    {
        RectTransform panel = Instantiate("Panel_Wide", hud);
        panel.name = showAll ? "CombineTablePanel" : "CombinePanel";
        panel.sizeDelta = new Vector2(1000, 860);
        Anchor(panel, new Vector2(0.5f, 0.5f), Vector2.zero);

        RectTransform title = NewRect("Title", panel);
        title.anchorMin = new Vector2(0, 1);
        title.anchorMax = new Vector2(1, 1);
        title.pivot = new Vector2(0.5f, 1);
        title.sizeDelta = new Vector2(0, 90);
        title.anchoredPosition = new Vector2(0, -20);
        Label(title, titleText, 48);

        Button close = Place("Button_Close", panel, new Vector2(1, 1), new Vector2(-24, -24), 80);

        // 보이는 영역(Viewport) 안에서 목록(List)이 위아래로 스크롤된다.
        RectTransform viewport = NewRect("Viewport", panel);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(48, 40);
        viewport.offsetMax = new Vector2(-48, -130);
        viewport.gameObject.AddComponent<RectMask2D>();
        // 빈 곳을 끌어도 스크롤되게 투명 이미지를 깐다.
        viewport.gameObject.AddComponent<Image>().color = Color.clear;

        RectTransform list = NewRect("List", viewport);
        list.anchorMin = new Vector2(0, 1);
        list.anchorMax = Vector2.one;
        list.pivot = new Vector2(0.5f, 1);
        list.sizeDelta = Vector2.zero;
        list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = list;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;

        VerticalLayoutGroup layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform empty = NewRect("Empty", list);
        empty.sizeDelta = new Vector2(0, 70);
        TMP_Text emptyText = Label(empty, "지금 만들 수 있는 조합이 없습니다", 36);

        RectTransform row = Instantiate("Panel_Steel", list);
        row.name = "RowTemplate";
        row.sizeDelta = new Vector2(0, 70);
        Button rowButton = row.gameObject.AddComponent<Button>();
        Label(row, "재료 + 재료  →  결과", 34);

        CombinePanel combinePanel = panel.gameObject.AddComponent<CombinePanel>();
        SerializedObject so = new SerializedObject(combinePanel);
        so.FindProperty("showAll").boolValue = showAll;
        so.FindProperty("list").objectReferenceValue = list;
        so.FindProperty("rowTemplate").objectReferenceValue = rowButton;
        so.FindProperty("emptyText").objectReferenceValue = emptyText;
        so.FindProperty("closeButton").objectReferenceValue = close;
        so.ApplyModifiedProperties();

        panel.gameObject.SetActive(false);
        return combinePanel;
    }

    private static TMP_Text Counter(string prefab, RectTransform parent, ref float x)
    {
        RectTransform rt = Instantiate(prefab, parent);
        FitHeight(rt, 100);
        Anchor(rt, new Vector2(0, 1), new Vector2(x, -10));
        x += rt.sizeDelta.x + 16f;
        return rt.GetComponentInChildren<TMP_Text>();
    }

    private static Button Place(string prefab, RectTransform parent, Vector2 corner, Vector2 offset, float height)
    {
        RectTransform rt = Instantiate(prefab, parent);
        FitHeight(rt, height);
        Anchor(rt, corner, offset);
        return rt.GetComponent<Button>();
    }

    private static Button TextButton(string name, string label, RectTransform parent, Vector2 offset)
    {
        RectTransform rt = Instantiate("Panel_Steel", parent);
        rt.name = name;
        rt.sizeDelta = new Vector2(220, 90);
        Anchor(rt, new Vector2(0.5f, 0), offset);
        Button button = rt.gameObject.AddComponent<Button>();
        Label(rt, label, 40);
        return button;
    }

    // 부모를 꽉 채우는 가운데 정렬 글자
    private static TMP_Text Label(RectTransform parent, string label, float size)
    {
        RectTransform textRect = NewRect("Label", parent);
        Stretch(textRect);
        TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        text.text = label;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private static RectTransform Instantiate(string prefab, RectTransform parent)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + prefab + ".prefab");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        Undo.RegisterCreatedObjectUndo(go, "HUD 배치");
        return (RectTransform)go.transform;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "HUD 배치");
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // corner 모서리에 붙이고, 그 모서리 기준으로 offset만큼 띄운다.
    private static void Anchor(RectTransform rt, Vector2 corner, Vector2 offset)
    {
        rt.anchorMin = corner;
        rt.anchorMax = corner;
        rt.pivot = corner;
        rt.anchoredPosition = offset;
    }

    private static void FitHeight(RectTransform rt, float height)
    {
        Vector2 size = rt.sizeDelta;
        rt.sizeDelta = new Vector2(size.x * height / size.y, height);
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/UI/" + name + ".png");
    }
}
