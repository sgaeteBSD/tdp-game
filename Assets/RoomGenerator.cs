using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; //assign room prefabs here
    public int level = 1;
    private int gridSize = 7;
    private int totalRooms;
    private Vector2Int center = new Vector2Int(3, 3);
    private HashSet<Vector2Int> placedRooms = new HashSet<Vector2Int>();
    private List<Vector2Int> deadEnds = new List<Vector2Int>();
    void Start()
    {
        GenerateRooms();
    }

    void GenerateRooms()
    {
        do
        {
            placedRooms.Clear();
            deadEnds.Clear();
            totalRooms = Mathf.CeilToInt(Random.Range(1, 3) * (16 + level) / 2);
            placedRooms.Add(center);
            ExpandFrom(center);
        }
        while (!IsValidGeneration());

        PopulateRooms();
    }

    void ExpandFrom(Vector2Int current)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2Int newPos = current + dir;
            if (placedRooms.Count >= totalRooms) break;
            if (placedRooms.Contains(newPos)) continue;
            if (GetNeighborCount(newPos) > 1) continue;
            if (Random.value < 0.5f) continue;

            placedRooms.Add(newPos);
            ExpandFrom(newPos);
        }
    }

    bool IsValidGeneration()
    {
        foreach (var pos in placedRooms)
        {
            if (GetNeighborCount(pos) == 1)
                deadEnds.Add(pos);
        }

        return placedRooms.Count >= 8 && deadEnds.Count > 1 && deadEnds.Exists(d => Vector2Int.Distance(d, center) >= 3);
    }

    int GetNeighborCount(Vector2Int pos)
    {
        int count = 0;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in directions)
        {
            if (placedRooms.Contains(pos + dir))
                count++;
        }
        return count;
    }

    void PopulateRooms()
    {
        Vector2Int worldOffset = new Vector2Int(center.x * 26, center.y * 16); //shift center to (0,0)

        foreach (var roomPos in placedRooms)
        {
            Vector3 worldPos = new Vector3((roomPos.x * 26) - worldOffset.x, (roomPos.y * 16) - worldOffset.y, 0);
            Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)], worldPos, Quaternion.identity);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}
