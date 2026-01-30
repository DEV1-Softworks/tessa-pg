using System.Numerics;
using UnityEngine;

public class TessaRoomData
{
    public Vector2Int Coordinates { get; }

    public RoomType Type { get; }

    public TessaRoomData(Vector2Int coordinates, RoomType type)
    {
        Coordinates = coordinates;
        Type = type;
    }
}
