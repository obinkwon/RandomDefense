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

    [MenuItem("RandomDefense/UI 프리팹 만들기")]
    internal static void Build()
    {
        Directory.CreateDirectory(PrefabDir);

        // 원형 버튼: 재생·일시정지·배속·설정·닫기
        foreach (string name in new[] { "play", "pause", "fast", "settings", "close" })
            Save(MakeButton("btn_" + name), "Button_" + Pascal(name));

        Save(MakeButton("btn_summon"), "Button_Summon");

        // 카운터: 왼쪽 아이콘 옆에 숫자
        foreach (string name in new[] { "crystal", "life", "stage", "enemy" })
            Save(MakeCounter("counter_" + name), "Counter_" + Pascal(name));

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

    private static GameObject MakeCounter(string spriteName)
    {
        GameObject go = MakeImage(spriteName);
        go.GetComponent<Image>().preserveAspect = true;

        GameObject label = new GameObject("Value", typeof(RectTransform));
        label.transform.SetParent(go.transform, false);

        // 왼쪽 40%는 아이콘 자리, 나머지에 숫자
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
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
