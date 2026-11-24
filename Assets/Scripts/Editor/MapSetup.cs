using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// MapLayout(글자 배열)으로 열린 씬에 Tilemap 맵, 직업 배치 칸(S), 적 순환 경로(R부터)를 깐다.
// 맵 가운데가 원점, 1칸 = 1유닛. 카메라는 맵 전체가 보이게 맞춘다.
public static class MapSetup
{
    private const string TileDir = "Assets/Data/Maps/Tiles/";

    [MenuItem("RandomDefense/맵 배치")]
    private static void Build()
    {
        SummonManager summon = Object.FindFirstObjectByType<SummonManager>();
        EnemySpawner spawner = Object.FindFirstObjectByType<EnemySpawner>();
        MapLayout layout = FindLayout();

        if (summon == null || spawner == null || layout == null)
        {
            Debug.LogError("InGame 씬을 열고, Data/Maps에 MapLayout 에셋이 있는지 확인하세요.");
            return;
        }

        string[] lines = layout.Lines();
        int height = lines.Length;
        int width = 0;
        foreach (string line in lines)
            width = Mathf.Max(width, line.Length);

        RemoveOld();

        // 맵: Grid + Tilemap. 그리드 왼쪽 아래를 (-w/2, -h/2)에 둬서 맵 가운데가 원점.
        GameObject map = new GameObject("Map", typeof(Grid));
        Undo.RegisterCreatedObjectUndo(map, "맵 배치");
        map.transform.position = new Vector3(-width / 2f, -height / 2f, 0f);

        GameObject ground = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
        ground.transform.SetParent(map.transform, false);
        Tilemap tilemap = ground.GetComponent<Tilemap>();
        ground.GetComponent<TilemapRenderer>().sortingOrder = -100;

        Dictionary<string, Sprite> decor = new Dictionary<string, Sprite>();
        foreach (MapLayout.Decor d in layout.decor)
            decor[d.symbol] = d.sprite;

        List<Transform> slots = new List<Transform>();
        Transform slotRoot = NewRoot("Slots");

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                char c = col < lines[row].Length ? lines[row][col] : '.';
                Vector3Int cell = new Vector3Int(col, height - 1 - row, 0);
                float angle = 0f;
                Sprite sprite;

                if (c == '=')
                    sprite = PathSprite(layout, lines, col, row, out angle);
                else if (c == 'R')
                    sprite = layout.spawn;
                else if (c == '#' || c == 'S')
                    sprite = layout.platform;
                else if (decor.TryGetValue(c.ToString(), out Sprite d))
                    sprite = d;
                else
                    sprite = layout.grass[Mathf.Abs(col * 73856093 ^ row * 19349663) % layout.grass.Length];

                tilemap.SetTile(cell, TileFor(sprite));
                if (angle != 0f)
                    tilemap.SetTransformMatrix(cell, Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle)));

                // 직업을 놓을 수 없는 발판(#)은 어둡게 해서 배치 칸(S)과 구분
                if (c == '#')
                    tilemap.SetColor(cell, new Color(0.55f, 0.55f, 0.55f));

                if (c == 'S')
                    slots.Add(NewPoint($"Slot_{slots.Count + 1:00}", slotRoot, CellCenter(col, row, width, height)));
            }
        }

        Transform pathRoot = NewRoot("Path");
        List<Transform> waypoints = new List<Transform>();
        foreach (Vector2Int p in MapLayout.TraceLoop(lines))
            waypoints.Add(NewPoint($"Waypoint_{waypoints.Count + 1}", pathRoot, CellCenter(p.x, p.y, width, height)));

        if (waypoints.Count < 2)
            Debug.LogError("R에서 시작하는 닫힌 길(=)을 찾지 못했습니다.");

        SetArray(summon, "slots", slots.ToArray());
        SetArray(spawner, "waypoints", waypoints.ToArray());
        FitCamera(height);

        EditorSceneManager.MarkSceneDirty(summon.gameObject.scene);
        Debug.Log($"맵 배치 완료: {width}×{height}칸, 배치 칸 {slots.Count}개, 경로 지점 {waypoints.Count}개. Ctrl+S로 저장하세요.");
    }

    private static MapLayout FindLayout()
    {
        string[] guids = AssetDatabase.FindAssets("t:MapLayout");
        return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<MapLayout>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    // 이웃한 길 칸을 보고 타일과 회전(반시계, 도)을 고른다. 원본 타일: 직선=세로, 모서리=오른쪽·아래, T=왼쪽·오른쪽·위.
    private static Sprite PathSprite(MapLayout layout, string[] lines, int col, int row, out float angle)
    {
        bool n = IsPathAt(lines, col, row - 1);
        bool s = IsPathAt(lines, col, row + 1);
        bool e = IsPathAt(lines, col + 1, row);
        bool w = IsPathAt(lines, col - 1, row);
        int count = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);
        angle = 0f;

        if (count == 4)
            return layout.pathCross;

        if (count == 3)
        {
            angle = !s ? 0f : !n ? 180f : !e ? 90f : -90f;
            return layout.pathT;
        }

        if (count == 2 && !(n && s) && !(e && w))
        {
            angle = e && s ? 0f : w && s ? -90f : w && n ? 180f : 90f;
            return layout.pathCorner;
        }

        if (e || w)
            angle = 90f;
        return layout.pathStraight;
    }

    private static bool IsPathAt(string[] lines, int col, int row)
    {
        return row >= 0 && row < lines.Length && col >= 0 && col < lines[row].Length && MapLayout.IsPath(lines[row][col]);
    }

    private static Vector2 CellCenter(int col, int row, int width, int height)
    {
        return new Vector2(col - width / 2f + 0.5f, (height - 1 - row) - height / 2f + 0.5f);
    }

    // 스프라이트마다 Tile 에셋 하나를 만들어 재사용한다.
    private static TileBase TileFor(Sprite sprite)
    {
        string path = TileDir + sprite.name + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            System.IO.Directory.CreateDirectory(TileDir);
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, path);
        }

        // 잠금이 있으면 칸별 회전(길)·색(배치 불가 발판)이 무시된다
        tile.flags = TileFlags.None;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    private static void FitCamera(int height)
    {
        // 씬 카메라에 MainCamera 태그가 없으면 Camera.main이 null이라 직접 찾는다.
        Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam == null)
            return;

        Undo.RecordObject(cam, "맵 배치");
        Undo.RecordObject(cam.transform, "맵 배치");
        Undo.RecordObject(cam.gameObject, "맵 배치");
        cam.gameObject.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = height / 2f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void RemoveOld()
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            // 부모를 먼저 지웠으면 자식은 이미 사라졌다.
            if (t == null)
                continue;

            string n = t.name;
            if (n == "Map" || n.StartsWith("CharacterSpawnPoint") || n == "SpawnPoint" || n == "TargetPoint" || n == "Slots" || n == "Path")
                Undo.DestroyObjectImmediate(t.gameObject);
        }
    }

    private static Transform NewRoot(string name)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "맵 배치");
        return go.transform;
    }

    private static Transform NewPoint(string name, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        Undo.RegisterCreatedObjectUndo(go, "맵 배치");
        return go.transform;
    }

    private static void SetArray(Object target, string field, Transform[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        prop.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

        so.ApplyModifiedProperties();
    }
}
