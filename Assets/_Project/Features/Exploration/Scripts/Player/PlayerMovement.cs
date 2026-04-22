using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool snapTo8Directions = true;

    private Rigidbody2D rb;
    private PlayerInputHandler inputHandler;
    private PlayerDash dash;

    public Vector2 LastMoveDir { get; private set; } = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        dash = GetComponent<PlayerDash>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (dash != null && dash.isDashing)
            return;

        Vector2 move = inputHandler.MoveInput;

        // Vector2 input = inputHandler.MoveInput;

        if (move.sqrMagnitude > 1f)
            move = move.normalized;

        Vector2 moveDir = snapTo8Directions ? SnapTo8(move) : move;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            LastMoveDir = moveDir;
        }

        rb.linearVelocity = moveDir * moveSpeed;
    }

    private Vector2 SnapTo8(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;

        float rad = snapped * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
}
