using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Art/Sprites/UI 스프라이트로 공통 UI 프리팹을 만든다 (Prefabs/UI). 다시 실행하면 덮어쓴다.
public static class UiPrefabBuilder
{
    private const string SpriteDir = "Assets/Art/Sprites/UI/";
    private const string PrefabDir = "Assets/Prefabs/UI/";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Regular SDF.asset";
    private const string CounterPanel = "counter_panel";

    [MenuItem("RandomDefense/UI 프리팹 만들기")]
    internal static void Build()
    {
        Directory.CreateDirectory(PrefabDir);

        // 원형 버튼: 재생·일시정지·배속·설정·닫기
        foreach (string name in new[] { "play", "pause", "fast", "normal_speed", "settings", "close" })
            Save(MakeButton("btn_" + name), "Button_" + Pascal(name));

        // 소환 버튼: 빈 카운터 패널 + 가운데 차원문 아이콘
        GameObject summon = MakeImage(CounterPanel);
        summon.GetComponent<Image>().preserveAspect = true;
        summon.AddComponent<Button>();
        AddIcon(summon, "icon_summon", new Vector2(0.3f, 0.14f), new Vector2(0.7f, 0.86f));
        Save(summon, "Button_Summon");

        // 카운터: 빈 카운터 패널 + 왼쪽 아이콘 + 오른쪽 숫자
        foreach (string name in new[] { "crystal", "barrier", "wave", "time", "monster" })
            Save(MakeCounter("icon_" + name), "Counter_" + Pascal(name));

        // 패널: 9-slice로 늘려 쓴다
        foreach (string name in new[] { "panel_wide", "panel_long", "panel_steel" })
        {
            GameObject go = MakeImage(name);
            Image image = go.GetComponent<Image>();
            image.type = Image.Type.Sliced;
            Save(go, "Panel_" + Pascal(name.Substring("panel_".Length)));
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"UI 프리팹을 {PrefabDir}에 만들었습니다.");
    }

    private static GameObject MakeImage(string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + spriteName + ".png");
        if (sprite == null)
            Debug.LogError($"스프라이트가 없습니다: {SpriteDir}{spriteName}.png");

        GameObject go = new GameObject(spriteName, typeof(RectTransform), typeof(Image));
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
        return go;
    }

    private static GameObject MakeButton(string spriteName)
    {
        GameObject go = MakeImage(spriteName);
        go.GetComponent<Image>().preserveAspect = true;
        go.AddComponent<Button>();
        return go;
    }

    // 패널 위에 아이콘을 얹는다. 32×32 아이콘을 anchor 영역에 맞춰 키운다 (Point 필터라 픽셀이 유지됨)
    private static void AddIcon(GameObject panel, string iconName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject icon = MakeImage(iconName);
        icon.name = "Icon";
        icon.transform.SetParent(panel.transform, false);
        Image image = icon.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        RectTransform rect = icon.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject MakeCounter(string iconName)
    {
        GameObject go = MakeImage(CounterPanel);
        go.GetComponent<Image>().preserveAspect = true;

        // 왼쪽 40%: 아이콘
        AddIcon(go, iconName, new Vector2(0.07f, 0.18f), new Vector2(0.38f, 0.82f));

        GameObject label = new GameObject("Value", typeof(RectTransform));
        label.transform.SetParent(go.transform, false);

        // 나머지: 숫자
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.4f, 0.15f);
        rect.anchorMax = new Vector2(0.92f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        text.text = "0";
        text.color = Color.white;
        text.enableAutoSizing = true;
        text.fontSizeMax = 72;
        text.alignment = TextAlignmentOptions.Right;
        return go;
    }

    private static void Save(GameObject go, string prefabName)
    {
        go.name = prefabName;
        PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + prefabName + ".prefab");
        Object.DestroyImmediate(go);
    }

    private static string Pascal(string s)
    {
        // normal_speed → NormalSpeed
        var sb = new System.Text.StringBuilder();
        foreach (string part in s.Split('_'))
            if (part.Length > 0)
                sb.Append(char.ToUpper(part[0])).Append(part.Substring(1));
        return sb.ToString();
    }
}
