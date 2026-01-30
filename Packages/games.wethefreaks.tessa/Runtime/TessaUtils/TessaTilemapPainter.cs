using UnityEngine;
using UnityEngine.Tilemaps;
using System.Numerics;
using System.Collections.Generic;

public class TessaTilemapPainter : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase gateTile;

    [Header("Room sizing (tiles)")]
    [SerializeField] private Vector2Int minRoomSizeTiles = new(8, 8);
    [SerializeField] private Vector2Int maxRoomSizeTiles = new(32, 18);
    [SerializeField] private int stepTiles = 8;
    [SerializeField] private int cellPaddingTiles = 2;

    private Vector2Int CellSizeTiles => new Vector2Int(
        maxRoomSizeTiles.x + cellPaddingTiles * 2,
        maxRoomSizeTiles.y + cellPaddingTiles * 2
    );

    public void Paint(
        Dictionary<Vector2Int, object> rooms,
        HashSet<(Vector2Int from, Vector2Int to)> roomConnections,
        HashSet<(Vector2Int from, Vector2Int to)> gatePositions
    )
    {
        if (floorTilemap == null || wallTilemap == null || floorTile == null || wallTile == null)
        {
            Debug.LogError("TessaTilemapPainter: You must assign all tilemaps or tiles before painting.");
            return;
        }

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        foreach (var roomEntry in rooms)
        {
            Vector2Int roomCoord = roomEntry.Key;
            Vector2Int roomSizeTiles = PickRoomSizeTiles();
            Vector2Int cellOriginTiles = new Vector2Int(roomCoord.x * CellSizeTiles.x, roomCoord.y * CellSizeTiles.y);

            Vector2Int roomOriginTiles = cellOriginTiles + new Vector2Int(
                (CellSizeTiles.x - roomSizeTiles.x) / 2,
                (CellSizeTiles.y - roomSizeTiles.y) / 2
            );

            PaintRoomRectangle(roomOriginTiles, roomSizeTiles);

            TryCarveDoor(roomCoord, roomOriginTiles, roomSizeTiles, Vector2Int.up, roomConnections, lockedConnections);
            TryCarveDoor(roomCoord, roomOriginTiles, roomSizeTiles, Vector2Int.right, roomConnections, lockedConnections);
            TryCarveDoor(roomCoord, roomOriginTiles, roomSizeTiles, Vector2Int.down, roomConnections, lockedConnections);
            TryCarveDoor(roomCoord, roomOriginTiles, roomSizeTiles, Vector2Int.left, roomConnections, lockedConnections);
        }
    }

    private Vector2Int PickRoomSizeTiles()
    {
        int randomWidth = RandomStepped(minRoomSizeTiles.x, maxRoomSizeTiles.x, stepTiles);
        int randomHeight = RandomStepped(minRoomSizeTiles.y, maxRoomSizeTiles.y, stepTiles);

        randomWidth = MathF.Max(4, randomWidth);
        randomHeight = MathF.Max(4, randomHeight);

        return new Vector2Int(randomWidth, randomHeight);
    }

    private static int RandomStepped(int minInclusive, int maxInclusive, int step)
    {
        if (step <= 0) step = 1;
        int minSteps = MathF.CeilToInt(minInclusive / (float)step);
        int maxSteps = MathF.FloorToInt(maxInclusive / (float)step);
        int chosenSteps = Random.Range(minSteps, maxSteps + 1);
        return chosenSteps * step;
    }

    private void PaintRoomRectangle(Vector2Int roomOriginTiles, Vector2Int roomSizeTiles)
    {
        int leftX = roomOriginTiles.x;
        int bottomY = roomOriginTiles.y;
        int rightX = roomOriginTiles.x + roomSizeTiles.x - 1;
        int topY = roomOriginTiles.y + roomSizeTiles.y - 1;

        // Flooor (inner fill)
        for (int tileX = leftX + 1; tileX <= rightX - 1; tileX++)
        {
            for (int tileY = bottomY + 1; tileY <= topY - 1; tileY++)
            {
                floorTilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);
            }
        }

        // Walls (borders)
        for (int tileX = leftX; tileX <= rightX; tileX++)
        {
            wallTilemap.SetTile(new Vector3Int(tileX, bottomY, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(tileX, topY, 0), wallTile);
        }

        for (int tileY = bottomY; tileY <= topY; tileY++)
        {
            wallTilemap.SetTile(new Vector3Int(leftX, tileY, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(rightX, tileY, 0), wallTile);
        }
    }

    private void TryCarveDoor(
        Vector2Int roomCoord,
        Vector2Int roomOriginTiles,
        Vector2Int roomSizeTiles,
        Vector2Int direction,
        HashSet<(Vector2Int from, Vector2Int to)> roomConnections,
        HashSet<(Vector2Int from, Vector2Int to)> lockedConnections
    )
    {
        Vector2Int neighborCoord = roomCoord + direction;

        bool hasConnection = roomConnections.Contains((roomCoord, neighborCoord)) ||
                             roomConnections.Contains((neighborCoord, roomCoord));

        if (!hasConnection) return;

        // Door at center of side
        Vector2Int doorTilePosition = GetDoorTilePosition(roomOriginTiles, roomSizeTiles, direction);

        // Open door = remove wall
        wallTilemap.SetTile(new Vector3Int(doorTilePosition.x, doorTilePosition.y, 0), null);

        bool isLocked = lockedConnections.Contains((roomCoord, neighborCoord)) ||
                        lockedConnections.Contains((neighborCoord, roomCoord));

        if (isLocked && gateTile != null)
        {
            wallTilemap.SetTile(new Vector3Int(doorTilePosition.x, doorTilePosition.y, 0), gateTile);
        }
    }

    private static Vector2Int GetDoorTilePosition(Vector2Int roomOriginTiles, Vector2Int roomSizeTiles, Vector2Int direction)
    {
        int leftX = roomOriginTiles.x;
        int bottomY = roomOriginTiles.y;
        int rightX = roomOriginTiles.x + roomSizeTiles.x - 1;
        int topY = roomOriginTiles.y + roomSizeTiles.y - 1;

        int centerX = roomOriginTiles.x + roomSizeTiles.x / 2;
        int centerY = roomOriginTiles.y + roomSizeTiles.y / 2;

        if (direction == Vector2Int.up) return new Vector2Int(centerX, topY);
        if (direction == Vector2Int.down) return new Vector2Int(centerX, bottomY);
        if (direction == Vector2Int.right) return new Vector2Int(rightX, centerY);

        // left
        return new Vector2Int(leftX, centerY);
    }
}
