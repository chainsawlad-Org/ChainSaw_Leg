using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions input;
    private Rigidbody2D rb;

    [SerializeField] private Transform aimTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("8 Direction Movement")]
    [SerializeField] private bool snapTo8Directions = true;

    private Vector2 moveInput = Vector2.zero;
    private Vector2 lastMoveDir = Vector2.up;

    private void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;
        input.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        UpdateAim();
    }

    private void Move()
    {
        Vector2 input = moveInput;

        if (input.sqrMagnitude > 1f)
            input = input.normalized;

        if (input.sqrMagnitude > 0.001f)
        {
            lastMoveDir = input;
        }

        Vector2 moveDir = snapTo8Directions ? SnapTo8(input) : input;

        rb.linearVelocity = moveDir * moveSpeed;
    }

    private Vector2 SnapTo8(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            return Vector2.zero;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;

        float rad = snapped * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    private void UpdateAim()
    {
        if (aimTransform == null) return;

        Vector2 dir = lastMoveDir;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        aimTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}