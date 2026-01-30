using System.Collections.Generic;
using UnityEngine;

public class TessaMetroidvaniaGenerator : MonoBehaviour
{
    [Header("Generation (Defaults)")]
    [Range(8, 20)] public int mainPathRoomCount = 12;
    [Range(1, 6)] public int optionalBranchCount = 3;

    [Header("Ability Gate")]
    public string unlockingAbilityId = "DoubleJump";
    public bool regenerateOnPlay = true;

    [Header("Gizmos (Editor Preview)")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool useGizmoSeed = true;
    [SerializeField] private int gizmoSeed = 12345;
    [SerializeField] private float gizmoCellSize = 1f;
    [SerializeField] private Color gizmoMainColor = new Color(0.35f, 0.85f, 0.95f, 0.8f);
    [SerializeField] private Color gizmoOptionalColor = new Color(0.35f, 0.95f, 0.55f, 0.8f);
    [SerializeField] private Color gizmoAbilityColor = new Color(0.95f, 0.7f, 0.2f, 0.9f);
    [SerializeField] private Color gizmoBossColor = new Color(0.95f, 0.3f, 0.3f, 0.9f);
    [SerializeField] private Color gizmoStartColor = new Color(0.95f, 0.95f, 0.95f, 0.9f);
    [SerializeField] private Color gizmoLockedColor = new Color(0.9f, 0.2f, 0.9f, 0.9f);

    private Edge lockedConnectionEdge;
    private TessaLevelLayout cachedGizmoLayout;
    private int cachedGizmoHash;

    [SerializeField] private TessaTilemapPainter tilemapPainter;

    private void Start()
    {
        if (regenerateOnPlay)
        {
            GenerateLevel();
        }
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (tilemapPainter == null)
        {
            Debug.LogError("TessaMetroidvaniaGenerator: TilemapPainter not assigned.");
            return;
        }

        var layout = BuildLayout(useSeed: false, seed: 0);
        tilemapPainter.PaintLevel(layout);
    }

    [ContextMenu("Randomize Gizmo Seed")]
    private void RandomizeGizmoSeed()
    {
        gizmoSeed = Random.Range(int.MinValue, int.MaxValue);
        cachedGizmoLayout = null;
    }

    private TessaLevelLayout BuildLayout(bool useSeed, int seed)
    {
        var occupiedCells = new HashSet<Vector2Int>();
        var mainPathCoords = new List<Vector2Int>();
        lockedConnectionEdge = default;

        Random.State previousState = Random.state;
        if (useSeed) Random.InitState(seed);

        var layout = new TessaLevelLayout();
        int mainPathLength = Mathf.Max(8, mainPathRoomCount);
        int maxAbilityIndexExclusive = Random.Range(4, Mathf.Min(7, mainPathLength - 2));
        int abilityRoomIndexOnMainPath = Random.Range(2, maxAbilityIndexExclusive);
        var optionalBranchEdges = new List<Edge>();
        int placementAttempts = 0;
        int branchesPlaced = 0;
        int maxPlacementAttempts = optionalBranchCount * 10;

        for (int x = 0; x < mainPathLength; x++)
        {
            Vector2Int roomCoordinates = new Vector2Int(x, 0);

            RoomType roomType = (x == 0) ? RoomType.Start :
                                (x == mainPathLength - 1) ? RoomType.Boss :
                                (x == abilityRoomIndexOnMainPath) ? RoomType.Ability :
                                RoomType.Normal;

            mainPathCoords.Add(roomCoordinates);
            occupiedCells.Add(roomCoordinates);
            layout.AddRoom(roomCoordinates, new TessaRoomData(roomCoordinates, roomType));
        }

        while (branchesPlaced < optionalBranchCount && placementAttempts < maxPlacementAttempts * 10)
        {
            placementAttempts++;

            int parentRoomIndexOnMainPath = Random.Range(2, mainPathLength - 2);
            var parentRoomCoord = mainPathCoords[parentRoomIndexOnMainPath];

            Vector2Int candidateUpCoord = parentRoomCoord + Vector2Int.up;
            Vector2Int candidateDownCoord = parentRoomCoord + Vector2Int.down;

            Vector2Int branchRoomCoord;
            if (!occupiedCells.Contains(candidateUpCoord)) branchRoomCoord = candidateUpCoord;
            else if (!occupiedCells.Contains(candidateDownCoord)) branchRoomCoord = candidateDownCoord;
            else continue;

            occupiedCells.Add(branchRoomCoord);
            layout.AddRoom(branchRoomCoord, new TessaRoomData(branchRoomCoord, RoomType.Optional));
            optionalBranchEdges.Add(new Edge(parentRoomCoord, branchRoomCoord, locked: false, requiresAbility: null));

            branchesPlaced++;
        }

        if (optionalBranchEdges.Count > 0)
        {
            var earlyBranchEdges = optionalBranchEdges.FindAll(edge => edge.FromCoord.x < abilityRoomIndexOnMainPath);

            Edge chosenEdgeToLock = (earlyBranchEdges.Count > 0) ?
                earlyBranchEdges[Random.Range(0, earlyBranchEdges.Count)] :
                optionalBranchEdges[Random.Range(0, optionalBranchEdges.Count)];

            lockedConnectionEdge = chosenEdgeToLock.WithLock(true, unlockingAbilityId);
        }

        for (int i = 0; i < mainPathCoords.Count - 1; i++)
        {
            var from = mainPathCoords[i];
            var to = mainPathCoords[i + 1];
            layout.AddConnection(from, to, locked: false);
        }

        foreach (var branchEdge in optionalBranchEdges)
        {
            bool isLocked = lockedConnectionEdge.IsValid &&
                            ((branchEdge.FromCoord == lockedConnectionEdge.FromCoord && branchEdge.ToCoord == lockedConnectionEdge.ToCoord) ||
                             (branchEdge.FromCoord == lockedConnectionEdge.ToCoord && branchEdge.ToCoord == lockedConnectionEdge.FromCoord));

            layout.AddConnection(branchEdge.FromCoord, branchEdge.ToCoord, isLocked, isLocked ? unlockingAbilityId : null);
        }

        if (useSeed) Random.state = previousState;
        return layout;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || Application.isPlaying) return;

        int currentHash = GetGizmoHash();
        if (cachedGizmoLayout == null || cachedGizmoHash != currentHash)
        {
            cachedGizmoHash = currentHash;
            int seed = useGizmoSeed ? gizmoSeed : Random.Range(int.MinValue, int.MaxValue);
            cachedGizmoLayout = BuildLayout(useSeed: true, seed: seed);
        }

        DrawGizmoLayout(cachedGizmoLayout);
    }

    private void DrawGizmoLayout(TessaLevelLayout layout)
    {
        if (layout == null) return;

        foreach (var room in layout.Rooms.Values)
        {
            Vector3 center = transform.position + new Vector3(room.Coordinates.x * gizmoCellSize, room.Coordinates.y * gizmoCellSize, 0f);
            Vector3 size = new Vector3(gizmoCellSize * 0.9f, gizmoCellSize * 0.9f, gizmoCellSize * 0.9f);
            Gizmos.color = RoomTypeToColor(room.Type);
            Gizmos.DrawWireCube(center, size);
        }

        foreach (var connection in layout.Connections)
        {
            Vector3 from = transform.position + new Vector3(connection.From.x * gizmoCellSize, connection.From.y * gizmoCellSize, 0f);
            Vector3 to = transform.position + new Vector3(connection.To.x * gizmoCellSize, connection.To.y * gizmoCellSize, 0f);
            Gizmos.color = connection.Locked ? gizmoLockedColor : gizmoMainColor;
            Gizmos.DrawLine(from, to);
        }
    }

    private Color RoomTypeToColor(RoomType type)
    {
        return type switch
        {
            RoomType.Start => gizmoStartColor,
            RoomType.Boss => gizmoBossColor,
            RoomType.Ability => gizmoAbilityColor,
            RoomType.Optional => gizmoOptionalColor,
            _ => gizmoMainColor
        };
    }

    private int GetGizmoHash()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + mainPathRoomCount;
            hash = hash * 31 + optionalBranchCount;
            hash = hash * 31 + (unlockingAbilityId != null ? unlockingAbilityId.GetHashCode() : 0);
            hash = hash * 31 + gizmoCellSize.GetHashCode();
            hash = hash * 31 + (useGizmoSeed ? 1 : 0);
            hash = hash * 31 + gizmoSeed;
            return hash;
        }
    }

    private readonly struct Edge
    {
        public readonly Vector2Int FromCoord;
        public readonly Vector2Int ToCoord;
        public readonly bool Locked;
        public readonly string RequiresAbility;
        public bool IsValid => FromCoord != ToCoord;

        public Edge(Vector2Int fromCoord, Vector2Int toCoord, bool locked, string requiresAbility)
        {
            FromCoord = fromCoord;
            ToCoord = toCoord;
            Locked = locked;
            RequiresAbility = requiresAbility;
        }

        public Edge WithLock(bool locked, string requiresAbility)
            => new Edge(FromCoord, ToCoord, locked, locked ? requiresAbility : null);
    }
}
