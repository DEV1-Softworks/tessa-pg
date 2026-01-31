using System.Collections.Generic;
using UnityEngine;

public class TessaGenerator : MonoBehaviour
{
    [Header("Generation (Defaults)")]
    [Range(8, 20)] public int mainPathRoomCount = 12;
    [Range(1, 6)] public int optionalBranchCount = 3;

    [Header("Ability Gate")]
    public string unlockingAbilityId = "DoubleJump";
    public bool regenerateOnPlay = true;

    [Header("Start Room Alignment")]
    [SerializeField] private bool alignStartRoomToCamera = true;
    [SerializeField] private Camera startCamera;

    private Edge lockedConnectionEdge;

    [SerializeField] private TessaMetroidvaniaTilemapPainter tilemapPainter;

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
            Debug.LogError("TessaGenerator: TilemapPainter not assigned.");
            return;
        }

        if (alignStartRoomToCamera)
        {
            Camera cameraToUse = startCamera != null ? startCamera : Camera.main;
            if (cameraToUse != null)
            {
                tilemapPainter.AlignStartRoomToWorldPosition(cameraToUse.transform.position);
            }
        }

        var layout = BuildLayout(useSeed: false, seed: 0);
        tilemapPainter.PaintLevel(layout);
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

        EnsureSingleBossRoom(layout, mainPathCoords[mainPathCoords.Count - 1]);

        if (useSeed) Random.state = previousState;
        return layout;
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

    private static void EnsureSingleBossRoom(TessaLevelLayout layout, Vector2Int bossCoord)
    {
        var rooms = layout.Rooms;

        foreach (var pair in rooms)
        {
            Vector2Int coord = pair.Key;
            RoomType type = pair.Value.Type;

            if (coord == bossCoord)
            {
                if (type != RoomType.Boss) rooms[coord] = new TessaRoomData(coord, RoomType.Boss);
                continue;
            }

            if (type == RoomType.Boss) rooms[coord] = new TessaRoomData(coord, RoomType.Normal);
        }
    }
}
