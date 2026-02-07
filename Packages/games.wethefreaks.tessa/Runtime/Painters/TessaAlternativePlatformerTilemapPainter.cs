using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;

// SOLID note: This MonoBehaviour combines template selection, validation,
// door carving, and tile painting in one class. Splitting these concerns would
// require new abstractions and Unity wiring not present in this package.
public class TessaAlternativePlatformerTilemapPainter : MonoBehaviour, ILevelPainter
{
    [Header("Tilemap references")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap hazardTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase hazardTile;

    [Header("Chunk size (tiles)")]
    [SerializeField] private int chunkWidth = 10;
    [SerializeField] private int chunkHeight = 8;

    [Header("Templates")]
    [SerializeField] private List<TessaRoomTemplate> templates = new();

    [Header("Door carving")]
    [SerializeField] private int doorWidth = 2;

    [Header("Grid limits (4x4 chunks)")]
    [SerializeField] private int gridWidth = 4;
    [SerializeField] private int gridHeight = 4;

    public void PaintLevel(TessaLevelLayout layout)
    {
        if (!ValidateSetup()) return;

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (hazardTilemap != null) hazardTilemap.ClearAllTiles();

        var coordMap = BuildNormalizedCoordMap(layout.Rooms, gridWidth, gridHeight);

        foreach (var room in layout.Rooms)
        {
            Vector2Int originalCoord = room.Key;
            Vector2Int roomCoord = coordMap[originalCoord];

            if (roomCoord.x < 0 || roomCoord.y < 0 || roomCoord.x >= gridWidth || roomCoord.y >= gridHeight)
            {
                Debug.LogWarning($"Room at {roomCoord} is out of grid bounds and will be skipped.");
                continue;
            }

            bool openNorth = HasNeighbor(layout, coordMap, roomCoord, Vector2Int.up);
            bool openEast = HasNeighbor(layout, coordMap, roomCoord, Vector2Int.right);
            bool openSouth = HasNeighbor(layout, coordMap, roomCoord, Vector2Int.down);
            bool openWest = HasNeighbor(layout, coordMap, roomCoord, Vector2Int.left);

            TessaRoomTemplate template = PickTemplate(openNorth, openEast, openSouth, openWest);

            if (template == null)
            {
                PaintFallbackRoom(roomCoord, openNorth, openEast, openSouth, openWest);
                continue;
            }

            PaintTemplate(roomCoord, template);
            CarveDoors(roomCoord, openNorth, openEast, openSouth, openWest);
        }
    }

    private bool ValidateSetup()
    {
        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("TessaAlternativePlatformerTilemapPainter: Tilemaps are not assigned.");
            return false;
        }

        if (floorTile == null || wallTile == null)
        {
            Debug.LogError("TessaAlternativePlatformerTilemapPainter: Tiles are not assigned.");
            return false;
        }

        if (templates == null) templates = new List<TessaRoomTemplate>();

        templates.RemoveAll(t => t == null || !t.IsValid() || t.width != chunkWidth || t.height != chunkHeight);

        if (doorWidth < 1) doorWidth = 1;
        if (doorWidth > chunkWidth - 2) doorWidth = chunkWidth - 2;

        return true;
    }

    private static bool HasNeighbor(
        TessaLevelLayout layout,
        Dictionary<Vector2Int, Vector2Int> coordMap,
        Vector2Int normalizedFrom,
        Vector2Int direction
    )
    {
        Vector2Int normalizedTo = normalizedFrom + direction;

        foreach (var c in layout.Connections)
        {
            Vector2Int from = coordMap[c.From];
            Vector2Int to = coordMap[c.To];

            if ((from == normalizedFrom && to == normalizedTo) ||
                (from == normalizedTo && to == normalizedFrom))
                return true;
        }
        return false;
    }


    private TessaRoomTemplate PickTemplate(bool openNorth, bool openEast, bool openSouth, bool openWest)
    {

        // Rule: Template must AT LEAST allow the required openings.
        // You may allow more; If you want to do it strictly, create more templates.

        var candidates = new List<TessaRoomTemplate>();

        foreach (var template in templates)
        {
            if (openNorth && !template.allowNorth) continue;
            if (openEast && !template.allowEast) continue;
            if (openSouth && !template.allowSouth) continue;
            if (openWest && !template.allowWest) continue;

            candidates.Add(template);
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void PaintTemplate(Vector2Int roomCoord, TessaRoomTemplate template)
    {
        Vector2Int origin = ChunkOrigin(roomCoord);

        for (int rowIndex = 0; rowIndex < chunkHeight; rowIndex++)
        {
            string row = template.rows[rowIndex];
            int tileY = origin.y + (chunkHeight - 1 - rowIndex); // Start from top row

            for (int tileXLocal = 0; tileXLocal < chunkWidth; tileXLocal++)
            {
                char ch = row[tileXLocal];
                int tileX = origin.x + tileXLocal;

                Vector3Int tilePosition = new Vector3Int(tileX, tileY, 0);

                switch (ch)
                {
                    case '#':
                        wallTilemap.SetTile(tilePosition, wallTile);
                        break;
                    case '.':
                        floorTilemap.SetTile(tilePosition, floorTile);
                        break;
                    case '^':
                        if (hazardTilemap != null)
                            hazardTilemap.SetTile(tilePosition, hazardTile);
                        else
                            floorTilemap.SetTile(tilePosition, floorTile);
                        break;
                    default:
                        // Empty space; do nothing
                        break;
                }
            }
        }
    }

    private void PaintFallbackRoom(Vector2Int roomCoord, bool openNorth, bool openEast, bool openSouth, bool openWest)
    {
        Vector2Int origin = ChunkOrigin(roomCoord);

        int leftX = origin.x;
        int bottomY = origin.y;
        int rightX = origin.x + chunkWidth - 1;
        int topY = origin.y + chunkHeight - 1;

        for (int x = leftX + 1; x <= rightX - 1; x++)
        {
            for (int y = bottomY + 1; y <= topY - 1; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }

        for (int x = leftX; x <= rightX; x++)
        {
            wallTilemap.SetTile(new Vector3Int(x, bottomY, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(x, topY, 0), wallTile);
        }

        for (int y = bottomY; y <= topY; y++)
        {
            wallTilemap.SetTile(new Vector3Int(leftX, y, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(rightX, y, 0), wallTile);
        }

        CarveDoors(roomCoord, openNorth, openEast, openSouth, openWest);
    }

    private void CarveDoors(Vector2Int roomCoord, bool openNorth, bool openEast, bool openSouth, bool openWest)
    {
        Vector2Int origin = ChunkOrigin(roomCoord);

        int leftX = origin.x;
        int bottomY = origin.y;
        int rightX = origin.x + chunkWidth - 1;
        int topY = origin.y + chunkHeight - 1;

        int centerX = origin.x + chunkWidth / 2;
        int centerY = origin.y + chunkHeight / 2;

        int halfDoorWidth = doorWidth / 2;

        if (openNorth)
        {
            for (int dx = -halfDoorWidth; dx < halfDoorWidth + (doorWidth % 2); dx++)
                wallTilemap.SetTile(new Vector3Int(centerX + dx, topY, 0), null);
        }

        if (openSouth)
        {
            for (int dx = -halfDoorWidth; dx < halfDoorWidth + (doorWidth % 2); dx++)
                wallTilemap.SetTile(new Vector3Int(centerX + dx, bottomY, 0), null);
        }

        if (openEast)
        {
            for (int dy = -halfDoorWidth; dy < halfDoorWidth + (doorWidth % 2); dy++)
                wallTilemap.SetTile(new Vector3Int(rightX, centerY + dy, 0), null);
        }

        if (openWest)
        {
            for (int dy = -halfDoorWidth; dy < halfDoorWidth + (doorWidth % 2); dy++)
                wallTilemap.SetTile(new Vector3Int(leftX, centerY + dy, 0), null);
        }
    }

    private Vector2Int ChunkOrigin(Vector2Int roomCoord)
    {
        return new Vector2Int(roomCoord.x * chunkWidth, roomCoord.y * chunkHeight);
    }

    private static Dictionary<Vector2Int, Vector2Int> BuildNormalizedCoordMap(
        IReadOnlyDictionary<Vector2Int, TessaRoomData> rooms,
        int gridWidth,
        int gridHeight
    )
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var coord in rooms.Keys)
        {
            if (coord.x < minX) minX = coord.x;
            if (coord.y < minY) minY = coord.y;
            if (coord.x > maxX) maxX = coord.x;
            if (coord.y > maxY) maxY = coord.y;
        }

        // Shifting for min to be at (0,0)
        Vector2Int shift = new Vector2Int(-minX, -minY);

        // If max is larger than grid limits, we need to scale down to fit (clamping)
        int width = (maxX - minX) + 1;
        int height = (maxY - minY) + 1;

        int overflowX = Mathf.Max(0, width - gridWidth);
        int overflowY = Mathf.Max(0, height - gridHeight);

        shift.x -= overflowX;
        shift.y -= overflowY;

        var map = new Dictionary<Vector2Int, Vector2Int>(rooms.Count);

        foreach (var original in rooms.Keys)
        {
            Vector2Int normalized = original + shift;

            normalized.x = Mathf.Clamp(normalized.x, 0, gridWidth - 1);
            normalized.y = Mathf.Clamp(normalized.y, 0, gridHeight - 1);

            map[original] = normalized;
        }

        return map;
    }
}
