using System.Collections.Generic;
using UnityEngine;

public class TessaRoomMakerGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject roomPrefab;

    [Header("Generation (Defaults)")]
    [Range(8, 20)] public int mainPathRoomCount = 12;
    [Range(1, 6)] public int optionalBranchCount = 3;
    public Vector2 gridCellSpacing = new Vector2(10f, 10f);

    [Header("Ability Gate")]
    public string unlockingAbilityId = "DoubleJump";
    public bool regenerateOnPlay = true;

    private readonly Dictionary<Vector2Int, TessaRoomInstance> occupiedCellsByCoord = new();
    private readonly List<Vector2Int> mainPathCoords = new();
    private Edge lockedConnectionEdge;

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
        ClearGeneratedRooms();

        occupiedCellsByCoord.Clear();
        mainPathCoords.Clear();
        lockedConnectionEdge = default;

        if (roomPrefab == null)
        {
            Debug.LogError("Room Prefab is not assigned. Please assign it in the inspector.");
            return;
        }

        int mainPathLength = Mathf.Max(8, mainPathRoomCount);
        int maxAbilityIndexExclusive = Random.Range(4, Mathf.Min(7, mainPathLength - 2));
        int abilityRoomIndexOnMainPath = Random.Range(2, maxAbilityIndexExclusive);

        for (int x = 0; x < mainPathLength; x++)
        {
            Vector2Int roomCoordinates = new Vector2Int(x, 0);

            RoomType roomType = (x == 0) ? RoomType.Start :
                                (x == mainPathLength - 1) ? RoomType.Boss :
                                (x == abilityRoomIndexOnMainPath) ? RoomType.Ability :
                                RoomType.Normal;

            CreateRoomAt(roomCoordinates, roomType);
            mainPathCoords.Add(roomCoordinates);
        }

        var optionalBranchEdges = new List<Edge>();

        int placementAttempts = 0;
        int branchesPlaced = 0;
        int maxPlacementAttempts = optionalBranchCount * 10;

        while (branchesPlaced < optionalBranchCount && placementAttempts < maxPlacementAttempts * 10)
        {
            placementAttempts++;

            int parentRoomIndexOnMainPath = Random.Range(2, mainPathLength - 2);
            var parentRoomCoord = mainPathCoords[parentRoomIndexOnMainPath];

            Vector2Int candidateUpCoord = parentRoomCoord + Vector2Int.up;
            Vector2Int candidateDownCoord = parentRoomCoord + Vector2Int.down;

            Vector2Int branchRoomCoord;

            if (!occupiedCellsByCoord.ContainsKey(candidateUpCoord)) branchRoomCoord = candidateUpCoord;
            else if (!occupiedCellsByCoord.ContainsKey(candidateDownCoord)) branchRoomCoord = candidateDownCoord;
            else continue;

            CreateRoomAt(branchRoomCoord, RoomType.Optional);

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

        foreach (var occupiedCell in occupiedCellsByCoord)
        {
            occupiedCell.Value.Spawn(transform, GridCoordToWorldPosition(occupiedCell.Key));
        }

        foreach (var occupiedCell in occupiedCellsByCoord)
        {
            ConnectDoorsForRoomAt(occupiedCell.Key);
        }

        if (lockedConnectionEdge.IsValid)
        {
            ApplyLockedDoor(lockedConnectionEdge);
        }
    }

    private Vector3 GridCoordToWorldPosition(Vector2Int gridCoord)
        => new Vector3(gridCoord.x * gridCellSpacing.x, 0f, gridCoord.y * gridCellSpacing.y);

    private void CreateRoomAt(Vector2Int roomCoordinates, RoomType roomType)
    {
        if (occupiedCellsByCoord.ContainsKey(roomCoordinates)) return;
        occupiedCellsByCoord[roomCoordinates] = new TessaRoomInstance(roomCoordinates, roomType, roomPrefab);
    }

    private void ConnectDoorsForRoomAt(Vector2Int roomCoordinates)
    {
        if (!occupiedCellsByCoord.TryGetValue(roomCoordinates, out var roomInstance) || roomInstance.View == null) return;

        SetDoorOpenState(roomInstance, Direction.North, occupiedCellsByCoord.ContainsKey(roomCoordinates + Vector2Int.up));
        SetDoorOpenState(roomInstance, Direction.East, occupiedCellsByCoord.ContainsKey(roomCoordinates + Vector2Int.right));
        SetDoorOpenState(roomInstance, Direction.South, occupiedCellsByCoord.ContainsKey(roomCoordinates + Vector2Int.down));
        SetDoorOpenState(roomInstance, Direction.West, occupiedCellsByCoord.ContainsKey(roomCoordinates + Vector2Int.left));
    }

    private void ApplyLockedDoor(Edge edge)
    {
        if (!occupiedCellsByCoord.TryGetValue(edge.FromCoord, out var fromRoom) || fromRoom.View == null) return;
        if (!occupiedCellsByCoord.TryGetValue(edge.ToCoord, out var toRoom) || toRoom.View == null) return;

        Direction directionFromFromToTo = DirectionFromTo(edge.FromCoord, edge.ToCoord);
        Direction directionFromToToFrom = DirectionFromTo(edge.ToCoord, edge.FromCoord);

        SetDoorLocked(fromRoom, directionFromFromToTo, edge.RequiresAbility);
        SetDoorLocked(toRoom, directionFromToToFrom, edge.RequiresAbility);
    }

    private static Direction DirectionFromTo(Vector2Int fromCoord, Vector2Int toCoord)
    {
        Vector2Int delta = toCoord - fromCoord;

        if (delta == Vector2Int.up) return Direction.North;
        if (delta == Vector2Int.right) return Direction.East;
        if (delta == Vector2Int.down) return Direction.South;

        return Direction.West;
    }

    private void SetDoorOpenState(TessaRoomInstance roomInstance, Direction direction, bool isOpen)
    {
        Transform doorTransform = roomInstance.GetDoor(direction);

        if (doorTransform == null) return;

        Collider2D collider2D = doorTransform.GetComponent<Collider2D>();

        if (collider2D != null)
        {
            collider2D.enabled = !isOpen;
        }

        doorTransform.gameObject.SetActive(true);
    }

    private void SetDoorLocked(TessaRoomInstance roomInstance, Direction direction, string requiredAbility)
    {
        Transform doorTransform = roomInstance.GetDoor(direction);
        if (doorTransform == null) return;

        Collider2D collider2D = doorTransform.GetComponent<Collider2D>();
        if (collider2D != null) collider2D.enabled = true;

        TessaAbilityGate2D gate = doorTransform.GetComponent<TessaAbilityGate2D>();
        if (gate != null) gate = doorTransform.gameObject.AddComponent<TessaAbilityGate2D>();
        gate.requiredAbilityId = requiredAbility;
    }

    private void ClearGeneratedRooms()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
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
