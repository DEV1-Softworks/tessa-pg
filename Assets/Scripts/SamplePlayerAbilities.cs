using System.Collections.Generic;
using UnityEngine;

public class SamplePlayerAbilities : MonoBehaviour, IAbilityReceiver
{
    [SerializeField] private List<string> abilities = new();

    public bool HasAbility(string id) => abilities.Contains(id);

    public void GrantAbility(string id)
    {
        if (!abilities.Contains(id)) abilities.Add(id);
    }
}
