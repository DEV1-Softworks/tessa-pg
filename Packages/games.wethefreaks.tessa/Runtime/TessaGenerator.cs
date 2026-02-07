using System.Collections.Generic;
using UnityEngine;

// SOLID note: This MonoBehaviour mixes generation orchestration, layout algorithms,
// Unity lifecycle concerns, and painter resolution. Fully separating these would
// require new services, DI, and editor wiring not present in this package.
public class TessaGenerator : MonoBehaviour
{
    [Header("Generation (Defaults)")]
    [Range(8, 20)] public int mainPathRoomCount = 12;
    [Range(1, 6)] public int optionalBranchCount = 3;

    [Header("Generation (Grid 4x4)")]
    [SerializeField] private bool useFixedGridLayout = true;
    [Range(2, 8)] [SerializeField] private int gridWidth = 4;
    [Range(2, 8)] [SerializeField] private int gridHeight = 4;
    [SerializeField] private bool startOnTopRow = true;
    [Range(0, 8)] [SerializeField] private int extraConnectionCount = 2;

    [Header("Ability Gate")]
    public string unlockingAbilityId = "DoubleJump";
    public bool regenerateOnPlay = true;

    private Edge lockedConnectionEdge;

    [Header("Painter")]
    [SerializeField] private MonoBehaviour painterBehaviour;
    private ILevelPainter tilemapPainter;

    private void OnValidate()
    {
        ResolvePainter();
    }

    private void Awake()
    {
        ResolvePainter();
    }

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
        ResolvePainter();
        if (tilemapPainter == null)
        {
            Debug.LogError("TessaGenerator: TilemapPainter not assigned.");
            return;
        }

        var layout = BuildLayout(useSeed: false, seed: 0);
        tilemapPainter.PaintLevel(layout);
    }

    private TessaLevelLayout BuildLayout(bool useSeed, int seed)
    {
        if (useFixedGridLayout)
        {
            return BuildGridLayout(useSeed, seed);
        }

        return BuildLinearLayout(useSeed, seed);
    }

    private void ResolvePainter()
    {
        if (painterBehaviour != null)
        {
            tilemapPainter = painterBehaviour as ILevelPainter;
            if (tilemapPainter == null)
            {
                Debug.LogError("TessaGenerator: PainterBehaviour does not implement ILevelPainter.");
            }
            return;
        }

        // Auto-resolve any MonoBehaviour on this GameObject that implements ILevelPainter.
        var components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component is ILevelPainter painter)
            {
                painterBehaviour = component;
                tilemapPainter = painter;
                return;
            }
        }

        tilemapPainter = null;
    }

    private TessaLevelLayout BuildGridLayout(bool useSeed, int seed)
    {
        lockedConnectionEdge = default;

        Random.State previousState = Random.state;
        if (useSeed) Random.InitState(seed);

        int width = Mathf.Max(2, gridWidth);
        int height = Mathf.Max(2, gridHeight);

        var layout = new TessaLevelLayout();
        var rooms = new Dictionary<Vector2Int, TessaRoomData>(width * height);
        var treeEdges = new List<Edge>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var coord = new Vector2Int(x, y);
                rooms[coord] = new TessaRoomData(coord, RoomType.Normal);
            }
        }

        BuildSpanningTree(width, height, treeEdges);

        var connections = new HashSet<(Vector2Int, Vector2Int)>();
        foreach (var edge in treeEdges)
        {
            AddConnectionKey(connections, edge.FromCoord, edge.ToCoord);
        }

        AddExtraConnections(width, height, connections, extraConnectionCount);

        int startRowY = startOnTopRow ? height - 1 : 0;
        int exitRowY = startOnTopRow ? 0 : height - 1;

        Vector2Int startCoord = new Vector2Int(Random.Range(0, width), startRowY);
        Vector2Int bossCoord = new Vector2Int(Random.Range(0, width), exitRowY);

        var path = GetPathInTree(width, height, treeEdges, startCoord, bossCoord);

        lockedConnectionEdge = PickLockedEdge(path, treeEdges);
        if (lockedConnectionEdge.IsValid && !string.IsNullOrEmpty(unlockingAbilityId))
        {
            lockedConnectionEdge = lockedConnectionEdge.WithLock(true, unlockingAbilityId);
        }

        Vector2Int abilityCoord = PickAbilityRoomCoord(width, height, path, startCoord, bossCoord);

        foreach (var pair in rooms)
        {
            layout.AddRoom(pair.Key, pair.Value);
        }

        if (rooms.ContainsKey(startCoord))
        {
            layout.AddRoom(startCoord, new TessaRoomData(startCoord, RoomType.Start));
        }

        if (rooms.ContainsKey(bossCoord))
        {
            layout.AddRoom(bossCoord, new TessaRoomData(bossCoord, RoomType.Boss));
        }

        if (rooms.ContainsKey(abilityCoord) && abilityCoord != startCoord && abilityCoord != bossCoord)
        {
            layout.AddRoom(abilityCoord, new TessaRoomData(abilityCoord, RoomType.Ability));
        }

        foreach (var connection in connections)
        {
            Vector2Int from = connection.Item1;
            Vector2Int to = connection.Item2;

            bool isLocked = lockedConnectionEdge.IsValid &&
                            ((from == lockedConnectionEdge.FromCoord && to == lockedConnectionEdge.ToCoord) ||
                             (from == lockedConnectionEdge.ToCoord && to == lockedConnectionEdge.FromCoord));

            layout.AddConnection(from, to, isLocked, isLocked ? unlockingAbilityId : null);
        }

        if (useSeed) Random.state = previousState;
        return layout;
    }

    private static void BuildSpanningTree(int width, int height, List<Edge> edges)
    {
        var visited = new HashSet<Vector2Int>();
        var stack = new Stack<Vector2Int>();

        Vector2Int start = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        visited.Add(start);
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            var unvisitedNeighbors = GetUnvisitedNeighbors(current, width, height, visited);

            if (unvisitedNeighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
            edges.Add(new Edge(current, next, locked: false, requiresAbility: null));
            visited.Add(next);
            stack.Push(next);
        }
    }

    private static List<Vector2Int> GetUnvisitedNeighbors(Vector2Int coord, int width, int height, HashSet<Vector2Int> visited)
    {
        var neighbors = new List<Vector2Int>(4);

        TryAddNeighbor(coord + Vector2Int.up, width, height, visited, neighbors);
        TryAddNeighbor(coord + Vector2Int.right, width, height, visited, neighbors);
        TryAddNeighbor(coord + Vector2Int.down, width, height, visited, neighbors);
        TryAddNeighbor(coord + Vector2Int.left, width, height, visited, neighbors);

        return neighbors;
    }

    private static void TryAddNeighbor(Vector2Int candidate, int width, int height, HashSet<Vector2Int> visited, List<Vector2Int> neighbors)
    {
        if (candidate.x < 0 || candidate.x >= width || candidate.y < 0 || candidate.y >= height) return;
        if (visited.Contains(candidate)) return;
        neighbors.Add(candidate);
    }

    private static void AddExtraConnections(int width, int height, HashSet<(Vector2Int, Vector2Int)> connections, int extraCount)
    {
        if (extraCount <= 0) return;

        var candidates = new List<(Vector2Int, Vector2Int)>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (x + 1 < width) candidates.Add((coord, new Vector2Int(x + 1, y)));
                if (y + 1 < height) candidates.Add((coord, new Vector2Int(x, y + 1)));
            }
        }

        Shuffle(candidates);

        int added = 0;
        foreach (var candidate in candidates)
        {
            if (added >= extraCount) break;
            if (connections.Contains(candidate) || connections.Contains((candidate.Item2, candidate.Item1))) continue;
            connections.Add(candidate);
            added++;
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void AddConnectionKey(HashSet<(Vector2Int, Vector2Int)> set, Vector2Int a, Vector2Int b)
    {
        if (set.Contains((a, b)) || set.Contains((b, a))) return;
        set.Add((a, b));
    }

    private static List<Vector2Int> GetPathInTree(int width, int height, List<Edge> edges, Vector2Int start, Vector2Int end)
    {
        var adjacency = new Dictionary<Vector2Int, List<Vector2Int>>(width * height);
        foreach (var edge in edges)
        {
            if (!adjacency.TryGetValue(edge.FromCoord, out var listA))
            {
                listA = new List<Vector2Int>();
                adjacency[edge.FromCoord] = listA;
            }
            if (!adjacency.TryGetValue(edge.ToCoord, out var listB))
            {
                listB = new List<Vector2Int>();
                adjacency[edge.ToCoord] = listB;
            }
            listA.Add(edge.ToCoord);
            listB.Add(edge.FromCoord);
        }

        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end) break;

            if (!adjacency.TryGetValue(current, out var neighbors)) continue;
            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor)) continue;
                visited.Add(neighbor);
                parent[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        var path = new List<Vector2Int>();
        if (!visited.Contains(end)) return path;

        Vector2Int step = end;
        path.Add(step);
        while (step != start)
        {
            step = parent[step];
            path.Add(step);
        }

        path.Reverse();
        return path;
    }

    private Edge PickLockedEdge(List<Vector2Int> path, List<Edge> treeEdges)
    {
        if (path.Count >= 4)
        {
            var eligible = new List<Edge>();
            for (int i = 2; i < path.Count - 1; i++)
            {
                Vector2Int from = path[i - 1];
                Vector2Int to = path[i];
                eligible.Add(new Edge(from, to, locked: false, requiresAbility: null));
            }

            if (eligible.Count > 0)
            {
                return eligible[Random.Range(0, eligible.Count)];
            }
        }

        if (treeEdges.Count > 0)
        {
            return treeEdges[Random.Range(0, treeEdges.Count)];
        }

        return default;
    }

    private static Vector2Int PickAbilityRoomCoord(int width, int height, List<Vector2Int> path, Vector2Int start, Vector2Int boss)
    {
        var pathSet = new HashSet<Vector2Int>(path);
        var offPath = new List<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (coord == start || coord == boss) continue;
                if (!pathSet.Contains(coord)) offPath.Add(coord);
            }
        }

        if (offPath.Count > 0)
        {
            return offPath[Random.Range(0, offPath.Count)];
        }

        if (path.Count > 2)
        {
            return path[Random.Range(1, path.Count - 1)];
        }

        return start;
    }

    private TessaLevelLayout BuildLinearLayout(bool useSeed, int seed)
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
