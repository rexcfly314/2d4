using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    private Rigidbody2D playerRigidbody;
    private Vector2 movementInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (playerRigidbody == null)
        {
            Debug.LogError(
                "PlayerMovement 初始化失敗，找不到 Rigidbody2D。",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        movementInput = ReadMovementInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private static Vector2 ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed)
        {
            input.y += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            input.y -= 1f;
        }

        if (keyboard.aKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            input.x += 1f;
        }

        return Vector2.ClampMagnitude(input, 1f);
    }

    private void Move()
    {
        playerRigidbody.linearVelocity =
            movementInput * moveSpeed;
    }
}