using UnityEngine;

public class TessaAbilityGate2D : MonoBehaviour
{
    public string requiredAbilityId = "DoubleJump";

    private void OnCollisionEnter2D(Collision2D collision) => TryUnlock(collision.collider);
    private void OnTriggerEnter2D(Collider2D other) => TryUnlock(other);

    private void TryUnlock(Collider2D collider)
    {
        var receiver = collider.GetComponent<IAbilityReceiver>();
        if (receiver == null) return;

        if (receiver.HasAbility(requiredAbilityId))
        {
            var collider2D = GetComponent<Collider2D>();
            if (collider2D != null) collider2D.enabled = false;
            enabled = false;
        }
    }
}
