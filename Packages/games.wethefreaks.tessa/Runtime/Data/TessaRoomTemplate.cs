using UnityEngine;

[CreateAssetMenu(fileName = "TessaRoomTemplate", menuName = "Tessa/Room Template")]
public class TessaRoomTemplate : ScriptableObject
{
    [Header("Template metadata")]
    public string templateId = "room_00";

    [Header("Template size")]
    public int width = 10;
    public int height = 8;

    [Header("Allowed connections")]
    public bool allowNorth, allowSouth, allowEast, allowWest;

    [Header("Rows (top to bottom). Use # for wall, . for floor, ^ for spike or hazard and ' ' for empty space.")]
    [TextArea(8, 20)]
    public string[] rows;

    public bool IsValid()
    {
        if (rows == null || rows.Length != height) return false;

        for (int y = 0; y < rows.Length; y++)
        {
            if (rows[y] == null || rows[y].Length != width) return false;
        }

        return true;
    }
}
