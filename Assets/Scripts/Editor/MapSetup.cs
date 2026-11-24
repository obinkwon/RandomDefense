using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 열린 씬에 맵 배경, 배치 칸 18개(6×3), 순환 경로 4점을 깐다.
// 좌표는 MapBackground.png 레이아웃(16×9칸, 1칸 = 1유닛, 가운데가 원점)과 같다.
public static class MapSetup
{
    private const string MapSpritePath = "Assets/Art/Sprites/Maps/MapBackground.png";

    [MenuItem("RandomDefense/맵 배치")]
    private static void Build()
    {
        SummonManager summon = Object.FindFirstObjectByType<SummonManager>();
        EnemySpawner spawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (summon == null || spawner == null)
        {
            Debug.LogError("InGame 씬을 연 상태에서 실행하세요.");
            return;
        }

        RemoveOldPoints();

        // 배경
        GameObject map = GameObject.Find("Map");
        if (map == null)
        {
            map = new GameObject("Map");
            Undo.RegisterCreatedObjectUndo(map, "맵 배치");
        }

        Undo.RecordObject(map.transform, "맵 배치");
        map.transform.position = Vector3.zero;

        if (!map.TryGetComponent(out SpriteRenderer sr))
            sr = Undo.AddComponent<SpriteRenderer>(map);

        Undo.RecordObject(sr, "맵 배치");
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapSpritePath);
        sr.sortingOrder = -100;

        // 배치 칸: 소환진 6×3 (x -2.5~2.5, y -1~1), 위 줄 왼쪽부터
        Transform[] slots = new Transform[18];
        Transform slotRoot = NewRoot("Slots");
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                int i = row * 6 + col;
                slots[i] = NewPoint($"Slot_{i + 1:00}", slotRoot, new Vector2(col - 2.5f, 1 - row));
            }
        }

        // 순환 경로: 균열(좌상)에서 시계 방향
        Transform pathRoot = NewRoot("Path");
        Transform[] waypoints =
        {
            NewPoint("Waypoint_1_Rift", pathRoot, new Vector2(-5.5f, 3f)),
            NewPoint("Waypoint_2", pathRoot, new Vector2(5.5f, 3f)),
            NewPoint("Waypoint_3", pathRoot, new Vector2(5.5f, -3f)),
            NewPoint("Waypoint_4", pathRoot, new Vector2(-5.5f, -3f)),
        };

        SetArray(summon, "slots", slots);
        SetArray(spawner, "waypoints", waypoints);

        // 카메라: 16:9 화면에 16×9칸이 꼭 맞게
        // 씬 카메라에 MainCamera 태그가 없으면 Camera.main이 null이라 직접 찾는다.
        Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            Undo.RecordObject(cam, "맵 배치");
            Undo.RecordObject(cam.transform, "맵 배치");
            Undo.RecordObject(cam.gameObject, "맵 배치");
            cam.gameObject.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        EditorSceneManager.MarkSceneDirty(summon.gameObject.scene);
        Debug.Log("맵 배치 완료. Ctrl+S로 씬을 저장하세요.");
    }

    private static void RemoveOldPoints()
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            // 부모를 먼저 지웠으면 자식은 이미 사라졌다.
            if (t == null)
                continue;

            string n = t.name;
            if (n.StartsWith("CharacterSpawnPoint") || n == "SpawnPoint" || n == "TargetPoint" || n == "Slots" || n == "Path")
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
