using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TessaTilemapPainter : MonoBehaviour, ILevelPainter
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap platformTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTopTile;
    [SerializeField] private TileBase wallBottomTile;
    [SerializeField] private TileBase wallLeftTile;
    [SerializeField] private TileBase wallRightTile;
    [SerializeField] private TileBase wallCornerTopLeftTile;
    [SerializeField] private TileBase wallCornerTopRightTile;
    [SerializeField] private TileBase wallCornerBottomLeftTile;
    [SerializeField] private TileBase wallCornerBottomRightTile;
    [SerializeField] private TileBase gateTile;
    [SerializeField] private TileBase platformTile;

    [Header("Room sizing (tiles)")]
    [SerializeField] private Vector2Int minRoomSizeTiles = new(8, 8);
    [SerializeField] private Vector2Int maxRoomSizeTiles = new(32, 18);
    [SerializeField] private bool useFixedRoomSize = true;
    [SerializeField] private int stepTiles = 8;
    [SerializeField] private int cellPaddingTiles = 0;

    [Header("Layout Offset (tiles)")]
    [SerializeField] private Vector2Int layoutOriginTiles = Vector2Int.zero;

    [Header("Corridors")]
    [SerializeField] private int corridorThicknessTiles = 2;

    [Header("Rendering (sorting)")]
    [SerializeField] private int floorSortingOrder = 0;
    [SerializeField] private int platformSortingOrder = 1;
    [SerializeField] private int wallSortingOrder = 2;

    [Header("Platforms (inner)")]
    [Range(0f, 1f)]
    [SerializeField] private float platformChance = 0.7f;
    [SerializeField] private Vector2Int platformsPerRoom = new(1, 3);
    [SerializeField] private Vector2Int platformLengthTiles = new(4, 10);
    [SerializeField] private int platformHorizontalPadding = 2;
    [SerializeField] private int platformVerticalPadding = 2;
    [SerializeField] private int platformMinVerticalSpacing = 2;

    private Tilemap PlatformTilemap => platformTilemap != null ? platformTilemap : wallTilemap;

    private Vector2Int CellSizeTiles => new Vector2Int(
        maxRoomSizeTiles.x + cellPaddingTiles * 2,
        maxRoomSizeTiles.y + cellPaddingTiles * 2
    );

    public void PaintLevel(TessaLevelLayout layout)
    {
        if (layout == null)
        {
            Debug.LogError("TessaTilemapPainter: Layout is null.");
            return;
        }

        Paint(layout.Rooms, layout.Connections);
    }

    public void Paint(
        Dictionary<Vector2Int, TessaRoomData> rooms,
        List<TessaConnection> connections
    )
    {
        if (floorTilemap == null || wallTilemap == null || floorTile == null)
        {
            Debug.LogError("TessaTilemapPainter: You must assign all tilemaps or tiles before painting.");
            return;
        }

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (platformTilemap != null && platformTilemap != wallTilemap)
        {
            platformTilemap.ClearAllTiles();
        }

        var placements = new Dictionary<Vector2Int, RoomPlacement>(rooms.Count);

        foreach (var roomEntry in rooms)
        {
            Vector2Int roomCoord = roomEntry.Key;
            Vector2Int roomSizeTiles = PickRoomSizeTiles();
            Vector2Int cellOriginTiles = layoutOriginTiles + new Vector2Int(roomCoord.x * CellSizeTiles.x, roomCoord.y * CellSizeTiles.y);

            Vector2Int roomOriginTiles = cellOriginTiles + new Vector2Int(
                (CellSizeTiles.x - roomSizeTiles.x) / 2,
                (CellSizeTiles.y - roomSizeTiles.y) / 2
            );

            placements[roomCoord] = new RoomPlacement(roomOriginTiles, roomSizeTiles);
            PaintRoomRectangle(roomOriginTiles, roomSizeTiles);
            if (roomEntry.Value.Type != RoomType.Boss)
            {
                PaintPlatforms(roomOriginTiles, roomSizeTiles);
            }
        }

        foreach (var connection in connections)
        {
            if (!placements.TryGetValue(connection.From, out var fromPlacement)) continue;
            if (!placements.TryGetValue(connection.To, out var toPlacement)) continue;

            CarveCorridor(connection.From, fromPlacement, connection.To, toPlacement);
        }
    }

    private Vector2Int PickRoomSizeTiles()
    {
        if (useFixedRoomSize) return maxRoomSizeTiles;

        int randomWidth = RandomStepped(minRoomSizeTiles.x, maxRoomSizeTiles.x, stepTiles);
        int randomHeight = RandomStepped(minRoomSizeTiles.y, maxRoomSizeTiles.y, stepTiles);

        randomWidth = Mathf.Max(4, randomWidth);
        randomHeight = Mathf.Max(4, randomHeight);

        return new Vector2Int(randomWidth, randomHeight);
    }

    private static int RandomStepped(int minInclusive, int maxInclusive, int step)
    {
        if (step <= 0) step = 1;
        int minSteps = Mathf.CeilToInt(minInclusive / (float)step);
        int maxSteps = Mathf.FloorToInt(maxInclusive / (float)step);
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
        for (int tileX = leftX + 1; tileX <= rightX - 1; tileX++)
        {
            if (wallBottomTile != null) wallTilemap.SetTile(new Vector3Int(tileX, bottomY, 0), wallBottomTile);
            if (wallTopTile != null) wallTilemap.SetTile(new Vector3Int(tileX, topY, 0), wallTopTile);
        }

        for (int tileY = bottomY + 1; tileY <= topY - 1; tileY++)
        {
            if (wallLeftTile != null) wallTilemap.SetTile(new Vector3Int(leftX, tileY, 0), wallLeftTile);
            if (wallRightTile != null) wallTilemap.SetTile(new Vector3Int(rightX, tileY, 0), wallRightTile);
        }

        if (wallCornerBottomLeftTile != null) wallTilemap.SetTile(new Vector3Int(leftX, bottomY, 0), wallCornerBottomLeftTile);
        if (wallCornerBottomRightTile != null) wallTilemap.SetTile(new Vector3Int(rightX, bottomY, 0), wallCornerBottomRightTile);
        if (wallCornerTopLeftTile != null) wallTilemap.SetTile(new Vector3Int(leftX, topY, 0), wallCornerTopLeftTile);
        if (wallCornerTopRightTile != null) wallTilemap.SetTile(new Vector3Int(rightX, topY, 0), wallCornerTopRightTile);
    }

    private void PaintPlatforms(Vector2Int roomOriginTiles, Vector2Int roomSizeTiles)
    {
        if (platformTile == null) return;
        if (Random.value > platformChance) return;

        int leftX = roomOriginTiles.x + platformHorizontalPadding + 1;
        int rightX = roomOriginTiles.x + roomSizeTiles.x - 1 - platformHorizontalPadding - 1;
        int bottomY = roomOriginTiles.y + platformVerticalPadding + 1;
        int topY = roomOriginTiles.y + roomSizeTiles.y - 1 - platformVerticalPadding - 1;

        int availableWidth = rightX - leftX + 1;
        if (availableWidth < platformLengthTiles.x || topY <= bottomY) return;

        int minPlatforms = Mathf.Clamp(platformsPerRoom.x, 0, platformsPerRoom.y);
        int maxPlatforms = Mathf.Max(minPlatforms, platformsPerRoom.y);
        int platformCount = Random.Range(minPlatforms, maxPlatforms + 1);

        var usedRows = new HashSet<int>();

        for (int i = 0; i < platformCount; i++)
        {
            int attempts = 0;
            int rowY;
            do
            {
                rowY = Random.Range(bottomY, topY + 1);
                attempts++;
            }
            while (IsRowTooClose(rowY, usedRows, platformMinVerticalSpacing) && attempts < 10);

            usedRows.Add(rowY);

            int maxLength = Mathf.Min(platformLengthTiles.y, availableWidth);
            int minLength = Mathf.Min(platformLengthTiles.x, maxLength);
            int length = Random.Range(minLength, maxLength + 1);
            int startX = Random.Range(leftX, rightX - length + 2);

            for (int x = startX; x < startX + length; x++)
            {
                PlatformTilemap.SetTile(new Vector3Int(x, rowY, 0), platformTile);
            }
        }
    }

    private static bool IsRowTooClose(int rowY, HashSet<int> usedRows, int minSpacing)
    {
        foreach (int used in usedRows)
        {
            if (Mathf.Abs(used - rowY) <= minSpacing) return true;
        }

        return false;
    }

    private void CarveCorridor(
        Vector2Int fromCoord,
        RoomPlacement fromPlacement,
        Vector2Int toCoord,
        RoomPlacement toPlacement
    )
    {
        int thickness = Mathf.Max(1, corridorThicknessTiles);

        if (fromCoord.x == toCoord.x)
        {
            bool fromBelow = fromCoord.y < toCoord.y;
            RoomPlacement bottomRoom = fromBelow ? fromPlacement : toPlacement;
            RoomPlacement topRoom = fromBelow ? toPlacement : fromPlacement;

            Vector2Int bottomDoor = GetDoorTilePosition(bottomRoom, Vector2Int.up);
            Vector2Int topDoor = GetDoorTilePosition(topRoom, Vector2Int.down);

            int xStart = bottomDoor.x - (thickness - 1) / 2;
            int xEnd = xStart + thickness - 1;
            int yStart = bottomDoor.y;
            int yEnd = topDoor.y;

            CarveRect(xStart, xEnd, yStart, yEnd, vertical: true);
        }
        else if (fromCoord.y == toCoord.y)
        {
            bool fromLeft = fromCoord.x < toCoord.x;
            RoomPlacement leftRoom = fromLeft ? fromPlacement : toPlacement;
            RoomPlacement rightRoom = fromLeft ? toPlacement : fromPlacement;

            Vector2Int leftDoor = GetDoorTilePosition(leftRoom, Vector2Int.right);
            Vector2Int rightDoor = GetDoorTilePosition(rightRoom, Vector2Int.left);

            int yStart = leftDoor.y - (thickness - 1) / 2;
            int yEnd = yStart + thickness - 1;
            int xStart = leftDoor.x;
            int xEnd = rightDoor.x;

            CarveRect(xStart, xEnd, yStart, yEnd, vertical: false);
        }
    }

    private void CarveRect(int xStart, int xEnd, int yStart, int yEnd, bool vertical)
    {
        int minX = Mathf.Min(xStart, xEnd);
        int maxX = Mathf.Max(xStart, xEnd);
        int minY = Mathf.Min(yStart, yEnd);
        int maxY = Mathf.Max(yStart, yEnd);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }

        if (vertical)
        {
            if (wallLeftTile != null)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    wallTilemap.SetTile(new Vector3Int(minX - 1, y, 0), wallLeftTile);
                }
            }

            if (wallRightTile != null)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    wallTilemap.SetTile(new Vector3Int(maxX + 1, y, 0), wallRightTile);
                }
            }
        }
        else
        {
            if (wallBottomTile != null)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    wallTilemap.SetTile(new Vector3Int(x, minY - 1, 0), wallBottomTile);
                }
            }

            if (wallTopTile != null)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    wallTilemap.SetTile(new Vector3Int(x, maxY + 1, 0), wallTopTile);
                }
            }

            if (wallCornerBottomLeftTile != null)
            {
                wallTilemap.SetTile(new Vector3Int(minX - 1, minY - 1, 0), wallCornerBottomLeftTile);
            }

            if (wallCornerTopLeftTile != null)
            {
                wallTilemap.SetTile(new Vector3Int(minX - 1, maxY + 1, 0), wallCornerTopLeftTile);
            }

            if (wallCornerBottomRightTile != null)
            {
                wallTilemap.SetTile(new Vector3Int(maxX + 1, minY - 1, 0), wallCornerBottomRightTile);
            }

            if (wallCornerTopRightTile != null)
            {
                wallTilemap.SetTile(new Vector3Int(maxX + 1, maxY + 1, 0), wallCornerTopRightTile);
            }
        }
    }

    private static Vector2Int GetDoorTilePosition(RoomPlacement room, Vector2Int direction)
    {
        int leftX = room.Origin.x;
        int bottomY = room.Origin.y;
        int rightX = room.Origin.x + room.Size.x - 1;
        int topY = room.Origin.y + room.Size.y - 1;

        int centerX = room.Origin.x + room.Size.x / 2;
        int centerY = room.Origin.y + room.Size.y / 2;

        if (direction == Vector2Int.up) return new Vector2Int(centerX, topY);
        if (direction == Vector2Int.down) return new Vector2Int(centerX, bottomY);
        if (direction == Vector2Int.right) return new Vector2Int(rightX, centerY);

        return new Vector2Int(leftX, centerY);
    }

    private void OnValidate()
    {
        minRoomSizeTiles = new Vector2Int(Mathf.Max(4, minRoomSizeTiles.x), Mathf.Max(4, minRoomSizeTiles.y));
        maxRoomSizeTiles = new Vector2Int(Mathf.Max(minRoomSizeTiles.x, maxRoomSizeTiles.x), Mathf.Max(minRoomSizeTiles.y, maxRoomSizeTiles.y));
        stepTiles = Mathf.Max(1, stepTiles);
        cellPaddingTiles = Mathf.Max(0, cellPaddingTiles);

        platformsPerRoom = new Vector2Int(Mathf.Max(0, platformsPerRoom.x), Mathf.Max(platformsPerRoom.x, platformsPerRoom.y));
        platformLengthTiles = new Vector2Int(Mathf.Max(2, platformLengthTiles.x), Mathf.Max(platformLengthTiles.x, platformLengthTiles.y));
        platformHorizontalPadding = Mathf.Max(0, platformHorizontalPadding);
        platformVerticalPadding = Mathf.Max(0, platformVerticalPadding);
        platformMinVerticalSpacing = Mathf.Max(0, platformMinVerticalSpacing);
        corridorThicknessTiles = Mathf.Max(1, corridorThicknessTiles);

        ApplySortingOrders();
    }

    private readonly struct RoomPlacement
    {
        public readonly Vector2Int Origin;
        public readonly Vector2Int Size;

        public RoomPlacement(Vector2Int origin, Vector2Int size)
        {
            Origin = origin;
            Size = size;
        }
    }

    private void Awake()
    {
        ApplySortingOrders();
    }

    private void ApplySortingOrders()
    {
        SetSortingOrder(floorTilemap, floorSortingOrder);
        SetSortingOrder(PlatformTilemap, platformSortingOrder);
        SetSortingOrder(wallTilemap, wallSortingOrder);
    }

    private static void SetSortingOrder(Tilemap tilemap, int order)
    {
        if (tilemap == null) return;
        var renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer == null) return;
        renderer.sortingOrder = order;
    }

    public void AlignStartRoomToWorldPosition(Vector3 worldPosition)
    {
        Tilemap reference = floorTilemap != null ? floorTilemap : wallTilemap;
        if (reference == null && platformTilemap != null) reference = platformTilemap;
        if (reference == null) return;

        Vector3Int cell = reference.WorldToCell(worldPosition);
        layoutOriginTiles = new Vector2Int(cell.x, cell.y);
    }
}
