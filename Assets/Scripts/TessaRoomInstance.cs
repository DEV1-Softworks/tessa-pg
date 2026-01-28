using UnityEngine;

internal sealed class TessaRoomInstance
{
    public readonly Vector2Int Coord;
    public readonly RoomType Type;
    public readonly GameObject Prefab;
    public GameObject View { get; private set; }
    private Transform doorNorth, doorEast, doorSouth, doorWest;

    public TessaRoomInstance(Vector2Int coord, RoomType type, GameObject prefab)
    {
        Coord = coord;
        Type = type;
        Prefab = prefab;
    }

    public void Spawn(Transform parent, Vector3 worldPosition)
    {
        View = Object.Instantiate(Prefab, worldPosition, Quaternion.identity, parent);
        View.name = $"Room_{Coord.x}_{Coord.y}_{Type}";

        doorNorth = View.transform.Find("Door_North");
        doorEast = View.transform.Find("Door_East");
        doorSouth = View.transform.Find("Door_South");
        doorWest = View.transform.Find("Door_West");

        Transform labelTransform = View.transform.Find("Label");

        if (labelTransform != null)
        {
            TextMesh textMesh = labelTransform.GetComponent<TextMesh>();
            if (textMesh != null) textMesh.text = Type.ToString();
        }

        TessaRoomMarker marker = View.GetComponent<TessaRoomMarker>();
        if (marker == null) marker = View.AddComponent<TessaRoomMarker>();
        marker.roomType = Type;
    }

    public Transform GetDoor(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return doorNorth;
            case Direction.East:
                return doorEast;
            case Direction.South:
                return doorSouth;

            default:
                return doorWest;
        }
    }
}
