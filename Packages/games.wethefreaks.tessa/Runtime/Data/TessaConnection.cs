using System.Collections.Generic;
using UnityEngine;

public class TessaConnection
{
    public Vector2Int From { get; }

    public Vector2Int To { get; }

    public bool Locked { get; }

    public string RequiredAbility { get; }

    public TessaConnection(Vector2Int from, Vector2Int to, bool locked, string requiredAbility = null)
    {
        From = from;
        To = to;
        Locked = locked;
        RequiredAbility = requiredAbility;
    }
}
