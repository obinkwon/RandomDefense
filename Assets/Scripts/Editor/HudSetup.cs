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
        TMP_Text life = Counter("Counter_Barrier", hud, ref x);
        TMP_Text stage = Counter("Counter_Wave", hud, ref x);
        TMP_Text time = Counter("Counter_Time", hud, ref x);
        TMP_Text enemy = Counter("Counter_Monster", hud, ref x);

        // 오른쪽 위: 설정, 배속, 일시정지 (오른쪽부터)
        Button settings = Place("Button_Settings", hud, new Vector2(1, 1), new Vector2(-20, -10), 100);
        Button fast = Place("Button_Fast", hud, new Vector2(1, 1), new Vector2(-130, -10), 100);
        Button pause = Place("Button_Pause", hud, new Vector2(1, 1), new Vector2(-240, -10), 100);

        // 아래쪽 한 줄: 조합표 · 소환 · 조합 · 취소(조합 모드에서만 보임)
        Button summon = Place("Button_Summon", hud, new Vector2(0.5f, 0), new Vector2(0, 8), 110);
        Button combine = TextButton("Button_Combine", "조합", hud, new Vector2(250, 14), 260);
        Button cancel = TextButton("Button_CancelCombine", "취소", hud, new Vector2(486, 14), 180);
        cancel.gameObject.SetActive(false);
        Button table = TextButton("Button_CombineTable", "조합표", hud, new Vector2(-250, 14), 260);

        // 안내 글자: 아래 버튼 줄 바로 위 가운데
        RectTransform toast = NewRect("Toast", hud);
        toast.sizeDelta = new Vector2(900, 60);
        Anchor(toast, new Vector2(0.5f, 0), new Vector2(0, 160));
        TMP_Text toastText = Label(toast, "", 32);
        toastText.fontStyle = FontStyles.Bold;
        toastText.outlineWidth = 0.2f;
        toastText.outlineColor = new Color32(0x1d, 0x14, 0x0d, 0xff);

        // 다른 HUD 위에 그려지도록 마지막에 만든다.
        RecipeTablePanel tablePanel = BuildRecipeTable(hud);

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
        so.FindProperty("cancelButton").objectReferenceValue = cancel;
        so.FindProperty("toastText").objectReferenceValue = toastText;
        so.FindProperty("combineModeSprite").objectReferenceValue = LoadSprite("btn_combine_mode");
        so.FindProperty("tableButton").objectReferenceValue = table;
        so.FindProperty("tablePanel").objectReferenceValue = tablePanel;
        so.FindProperty("pauseSprite").objectReferenceValue = LoadSprite("btn_pause");
        so.FindProperty("playSprite").objectReferenceValue = LoadSprite("btn_play");
        so.FindProperty("fastSprite").objectReferenceValue = LoadSprite("btn_fast");
        so.FindProperty("normalSprite").objectReferenceValue = LoadSprite("btn_normal_speed");
        so.ApplyModifiedProperties();

        // 조합표와 선택 표시 그림 연결 (씬 파일을 직접 고치면 열린 씬 저장 때 덮어써진다)
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
            summonSo.FindProperty("selectSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Effects/unit_select.png");
            summonSo.FindProperty("rangeFillSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Effects/range_fill.png");
            summonSo.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("HUD 배치 완료. 설정 버튼은 아직 기능이 연결되지 않았습니다. Ctrl+S로 저장하세요.");
    }

    // 조합표 창 (공통 에셋 시안 "조합표 팝업"을 1.6배: 1200×675 → 1920×1080).
    // 위쪽 정보 칸과 아래쪽 버튼 줄 사이를 어둡게 가리고, 그 위에 창을 띄운다. 탭·카드는 RecipeTablePanel이 열 때 만든다.
    private static RecipeTablePanel BuildRecipeTable(RectTransform hud)
    {
        RectTransform dim = NewRect("CombineTableDim", hud);
        dim.anchorMin = Vector2.zero;
        dim.anchorMax = Vector2.one;
        dim.offsetMin = new Vector2(0, 154);
        dim.offsetMax = new Vector2(0, -122);
        dim.gameObject.AddComponent<Image>().color = new Color(10 / 255f, 8 / 255f, 6 / 255f, 0.55f);
        Button overlay = dim.gameObject.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        dim.gameObject.SetActive(false);

        RectTransform panel = NewRect("CombineTablePanel", hud);
        panel.anchorMin = new Vector2(0, 1);
        panel.anchorMax = new Vector2(1, 1);
        panel.pivot = new Vector2(0.5f, 1);
        panel.offsetMin = new Vector2(58, -134 - 779);
        panel.offsetMax = new Vector2(-58, -134);
        Sliced(panel, "frame_panel", 1.5f);

        // 머리줄: 제목, 재료 거르기 칩, 닫기
        RectTransform header = NewRect("Header", panel);
        header.anchorMin = new Vector2(0, 1);
        header.anchorMax = new Vector2(1, 1);
        header.pivot = new Vector2(0.5f, 1);
        header.offsetMin = new Vector2(54, -42 - 77);
        header.offsetMax = new Vector2(-54, -42);

        RectTransform titleRect = NewRect("Title", header);
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = new Vector2(0, 1);
        titleRect.pivot = new Vector2(0, 0.5f);
        titleRect.sizeDelta = new Vector2(140, 0);
        TMP_Text titleText = Label(titleRect, "조합표", 35);
        titleText.color = new Color32(0xff, 0xe2, 0xa8, 0xff);
        titleText.fontStyle = FontStyles.Bold;

        RectTransform chip = NewRect("FilterChip", header);
        chip.anchorMin = new Vector2(0, 0.5f);
        chip.anchorMax = new Vector2(0, 0.5f);
        chip.pivot = new Vector2(0, 0.5f);
        chip.sizeDelta = new Vector2(320, 70);
        chip.anchoredPosition = new Vector2(160, 0);
        Image chipImage = Sliced(chip, "frame_card", 3f);
        Button chipButton = chip.gameObject.AddComponent<Button>();
        chipButton.targetGraphic = chipImage;
        Label(chip, "재료:", 22);
        chip.gameObject.SetActive(false);

        Button close = Place("Button_Close", header, new Vector2(1, 0.5f), Vector2.zero, 77);

        // 왼쪽 세로 등급 탭
        RectTransform tabs = NewRect("Tabs", panel);
        tabs.anchorMin = Vector2.zero;
        tabs.anchorMax = new Vector2(0, 1);
        tabs.pivot = new Vector2(0, 0.5f);
        tabs.offsetMin = new Vector2(54, 54);
        tabs.offsetMax = new Vector2(54 + 192, -132);
        VerticalLayoutGroup tabLayout = tabs.gameObject.AddComponent<VerticalLayoutGroup>();
        tabLayout.spacing = 13;
        tabLayout.childControlWidth = true;
        tabLayout.childControlHeight = false;
        tabLayout.childForceExpandWidth = true;
        tabLayout.childForceExpandHeight = false;

        // 오른쪽 2열 카드 목록. 보이는 영역(Viewport) 안에서 위아래로 스크롤된다.
        RectTransform viewport = NewRect("Viewport", panel);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(54 + 192 + 16, 54);
        viewport.offsetMax = new Vector2(-54, -132);
        viewport.gameObject.AddComponent<RectMask2D>();
        // 빈 곳을 끌어도 스크롤되게 투명 이미지를 깐다.
        viewport.gameObject.AddComponent<Image>().color = Color.clear;

        RectTransform list = NewRect("List", viewport);
        list.anchorMin = new Vector2(0, 1);
        list.anchorMax = Vector2.one;
        list.pivot = new Vector2(0.5f, 1);
        list.sizeDelta = Vector2.zero;
        list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GridLayoutGroup grid = list.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(600, 147);
        grid.spacing = new Vector2(13, 13);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = list;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;

        RectTransform empty = NewRect("Empty", viewport);
        empty.anchorMin = new Vector2(0, 1);
        empty.anchorMax = Vector2.one;
        empty.pivot = new Vector2(0.5f, 1);
        empty.sizeDelta = new Vector2(0, 120);
        TMP_Text emptyText = Label(empty, "조건에 맞는 조합이 없어요", 22);

        RecipeTablePanel table = panel.gameObject.AddComponent<RecipeTablePanel>();
        SerializedObject so = new SerializedObject(table);
        so.FindProperty("tabs").objectReferenceValue = tabs;
        so.FindProperty("list").objectReferenceValue = grid;
        so.FindProperty("emptyText").objectReferenceValue = emptyText;
        so.FindProperty("filterChip").objectReferenceValue = chipButton;
        so.FindProperty("closeButton").objectReferenceValue = close;
        so.FindProperty("overlay").objectReferenceValue = overlay;
        so.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        so.FindProperty("cardFrame").objectReferenceValue = LoadSprite("frame_card");
        so.FindProperty("lessThan").objectReferenceValue = LoadSprite("symbol_lt");
        so.FindProperty("plus").objectReferenceValue = LoadSprite("symbol_plus");
        so.FindProperty("question").objectReferenceValue = LoadSprite("symbol_question");
        so.ApplyModifiedProperties();

        panel.gameObject.SetActive(false);
        return table;
    }

    // 9칸 나눔 액자를 부모 크기에 맞춰 깐다. multiplier가 클수록 모서리가 작게 그려진다.
    private static Image Sliced(RectTransform rt, string sprite, float multiplier)
    {
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = LoadSprite(sprite);
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = multiplier;
        return image;
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

    private static Button TextButton(string name, string label, RectTransform parent, Vector2 offset, float width)
    {
        // 돌 테두리 패널을 9칸 나눔으로 늘려서 글자 버튼 틀로 쓴다 (모서리 장식이 찌그러지지 않게).
        RectTransform rt = NewRect(name, parent);
        rt.sizeDelta = new Vector2(width, 90);
        Image frame = Sliced(rt, "panel_stone", 2f);
        Anchor(rt, new Vector2(0.5f, 0), offset);
        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = frame;
        // "조합하기 0/4"도 돌 테두리 안에 들어가는 크기
        Label(rt, label, 34);
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
