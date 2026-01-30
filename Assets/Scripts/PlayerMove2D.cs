using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    public float speed = 6f;
    public float jumpImpulse = 12f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // Update is called once per frame
    private void Update()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(move.x * speed, rb.linearVelocity.y);

        if (inputActions.Player.Jump.WasPressedThisFrame())
        {
            rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
        }

    }
}
