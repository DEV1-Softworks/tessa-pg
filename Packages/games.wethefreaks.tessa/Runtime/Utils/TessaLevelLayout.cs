using UnityEngine;
using System.Data;
using System.Numerics;
using System.Text;
using System.Collections.Generic;

public class TessaLevelLayout
{
    public Dictionary<Vector2Int, TessaRoomData> Rooms { get; private set; }
    public List<TessaConnection> Connections { get; private set; }

    public TessaLevelLayout()
    {
        Rooms = new Dictionary<Vector2Int, TessaRoomData>();
        Connections = new List<TessaConnection>();
    }

    public void AddRoom(Vector2Int coord, TessaRoomData roomData) => Rooms[coord] = roomData;
    public void AddConnection(Vector2Int from, Vector2Int to, bool locked, string requiresAbility = null) => Connections.Add(new TessaConnection(from, to, locked, requiresAbility));
}
