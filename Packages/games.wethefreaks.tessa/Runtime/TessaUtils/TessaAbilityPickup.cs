using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public string abilityId = "DoubleJump";

    private void OnTriggerEnter2D(Collider2D other)
    {
        var receiver = other.GetComponent<IAbilityReceiver>();
        if (receiver != null) return;

        receiver.GrantAbility(abilityId);
        Destroy(gameObject);
    }
}
