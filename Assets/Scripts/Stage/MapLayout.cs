using System;
using System.Collections.Generic;
using UnityEngine;

// 글자 배열로 적는 맵. 한 글자 = 한 칸(1유닛), 맨 위 줄이 화면 맨 위.
//   .  풀       =  길(이웃 칸을 보고 직선·모서리·T자·십자를 자동으로 고른다)
//   R  적 출발점(길로 취급, 여기서 시계 방향으로 돈다)
//   #  발판     S  발판 + 직업 배치 칸
//   그 밖의 글자는 decor 목록에서 찾는다 (나무, 바위 등).
// 메뉴 RandomDefense > 맵 배치 가 이 데이터로 Tilemap, 배치 칸, 적 경로를 만든다.
[CreateAssetMenu(fileName = "MapLayout", menuName = "Random Job Defense/Map Layout")]
public class MapLayout : ScriptableObject
{
    [Serializable]
    public class Decor
    {
        public string symbol;
        public Sprite sprite;
    }

    [TextArea(9, 30)] public string rows;

    [Header("Tiles")]
    public Sprite[] grass;
    [Tooltip("세로 방향 직선")] public Sprite pathStraight;
    [Tooltip("오른쪽·아래로 이어진 모서리")] public Sprite pathCorner;
    [Tooltip("왼쪽·오른쪽·위로 이어진 T자")] public Sprite pathT;
    public Sprite pathCross;
    public Sprite spawn;
    public Sprite platform;
    public Decor[] decor;

    // 줄 끝 공백·빈 줄을 정리한 글자 배열. [0]이 맨 위 줄.
    public string[] Lines()
    {
        List<string> lines = new List<string>();
        foreach (string line in rows.Split('\n'))
        {
            string trimmed = line.TrimEnd('\r', ' ');
            if (trimmed.Length > 0)
                lines.Add(trimmed);
        }

        return lines.ToArray();
    }

    public static bool IsPath(char c)
    {
        return c == '=' || c == 'R';
    }

    // 출발점 R에서 길을 따라 한 바퀴 돌며 방향이 꺾이는 칸을 모은다. 결과 [0]은 R.
    // 좌표는 (열, 위에서부터 행). 오른쪽 → 아래 → 왼쪽 → 위 순서로 먼저 가 본다(시계 방향).
    public static List<Vector2Int> TraceLoop(string[] lines)
    {
        Vector2Int start = new Vector2Int(-1, -1);
        for (int y = 0; y < lines.Length; y++)
        {
            int x = lines[y].IndexOf('R');
            if (x >= 0)
                start = new Vector2Int(x, y);
        }

        List<Vector2Int> corners = new List<Vector2Int>();
        if (start.x < 0)
            return corners;

        Vector2Int[] dirs = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down }; // up = 아래 행(+y)
        Vector2Int pos = start;
        Vector2Int prevDir = Vector2Int.zero;
        corners.Add(start);

        for (int step = 0; step < 10000; step++)
        {
            Vector2Int dir = Vector2Int.zero;
            foreach (Vector2Int d in dirs)
            {
                if (d == -prevDir)
                    continue;

                Vector2Int n = pos + d;
                if (n.y >= 0 && n.y < lines.Length && n.x >= 0 && n.x < lines[n.y].Length && IsPath(lines[n.y][n.x]))
                {
                    dir = d;
                    break;
                }
            }

            if (dir == Vector2Int.zero)
                break; // 막다른 길

            if (prevDir != Vector2Int.zero && dir != prevDir && pos != start)
                corners.Add(pos);

            pos += dir;
            prevDir = dir;

            if (pos == start)
                break;
        }

        return corners;
    }
}
