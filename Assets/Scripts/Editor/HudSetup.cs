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

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Button_Play.prefab") == null)
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
        TMP_Text enemy = Counter("Counter_Enemy", hud, ref x);

        // 오른쪽 위: 설정, 배속, 일시정지 (오른쪽부터)
        Button settings = Place("Button_Settings", hud, new Vector2(1, 1), new Vector2(-20, -10), 100);
        Button fast = Place("Button_Fast", hud, new Vector2(1, 1), new Vector2(-130, -10), 100);
        Button pause = Place("Button_Pause", hud, new Vector2(1, 1), new Vector2(-240, -10), 100);

        // 아래쪽 한 줄: 조합표 · 소환 · 조합
        Button summon = Place("Button_Summon", hud, new Vector2(0.5f, 0), new Vector2(0, 8), 110);
        Button combine = TextButton("Button_Combine", "조합", hud, new Vector2(230, 14));
        Button table = TextButton("Button_CombineTable", "조합표", hud, new Vector2(-230, 14));

        SerializedObject so = new SerializedObject(gameUI);
        so.FindProperty("crystalText").objectReferenceValue = crystal;
        so.FindProperty("lifeText").objectReferenceValue = life;
        so.FindProperty("stageText").objectReferenceValue = stage;
        so.FindProperty("enemyText").objectReferenceValue = enemy;
        so.FindProperty("summonButton").objectReferenceValue = summon;
        so.FindProperty("pauseButton").objectReferenceValue = pause;
        so.FindProperty("fastButton").objectReferenceValue = fast;
        so.FindProperty("pauseSprite").objectReferenceValue = LoadSprite("btn_pause");
        so.FindProperty("playSprite").objectReferenceValue = LoadSprite("btn_play");
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("HUD 배치 완료. 조합·조합표·설정 버튼은 아직 기능이 연결되지 않았습니다. Ctrl+S로 저장하세요.");
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

        RectTransform textRect = NewRect("Label", rt);
        Stretch(textRect);
        TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        text.text = label;
        text.fontSize = 40;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        return button;
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
