using System.Collections.Generic;
using UnityEngine;

public class TessaPlayerAbilities : MonoBehaviour
{
    [SerializeField] private List<string> abilities = new();

    public bool Has(string id) => abilities.Contains(id);

    public void Grant(string id)
    {
        if (!abilities.Contains(id)) abilities.Add(id);
    }
}
