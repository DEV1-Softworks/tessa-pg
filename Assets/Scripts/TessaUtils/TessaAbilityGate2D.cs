using UnityEngine;

public class TessaAbilityGate2D : MonoBehaviour
{
    public string requiredAbilityId = "DoubleJump";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryUnlock(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryUnlock(other);
    }

    private void TryUnlock(Collider2D collider)
    {
        TessaPlayerAbilities playerAbilities = collider.GetComponent<TessaPlayerAbilities>();
        if (playerAbilities == null) return;

        if (playerAbilities.Has(requiredAbilityId))
        {
            Collider2D collider2D = GetComponent<Collider2D>();
            if (collider2D != null) collider2D.enabled = false;

            enabled = false;
        }
    }
}
