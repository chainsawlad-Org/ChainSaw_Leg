using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool snapTo8Directions = true;

    [Header("Smoothing")]
    [SerializeField] private float moveAcceleration = 130f;
    [SerializeField] private float moveDeceleration = 700f;
    [SerializeField] private float reverseDirectionAcceleration = 220f;

    [SerializeField] private PlayerFallStateOrchestrator playerFallStateOrchestrator;

    private Rigidbody2D rb;
    private PlayerInputHandler inputHandler;
    private PlayerDash dash;
    private Vector2 currentVelocity;

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
        if (DialogueManager.Instance != null && DialogueManager.Instance.BlockGameplayInput)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerFallStateOrchestrator != null && playerFallStateOrchestrator.IsFalling)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (dash != null && dash.isDashing)
        {
            return;
        }

        Vector2 move = inputHandler.MoveInput;

        if (move.sqrMagnitude > 1f)
        {
            move = move.normalized;
        }

        Vector2 moveDir = snapTo8Directions ? SnapTo8(move) : move;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            LastMoveDir = moveDir;
        }

        Vector2 targetVelocity = moveDir * moveSpeed;
        float acceleration = GetVelocityChangeRate(targetVelocity);

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = currentVelocity;
    }

    private float GetVelocityChangeRate(Vector2 targetVelocity)
    {
        if (targetVelocity.sqrMagnitude <= 0.0001f)
        {
            return moveDeceleration;
        }

        if (currentVelocity.sqrMagnitude <= 0.0001f)
        {
            return moveAcceleration;
        }

        float directionDot = Vector2.Dot(
            currentVelocity.normalized,
            targetVelocity.normalized);

        if (directionDot < 0f)
        {
            return reverseDirectionAcceleration;
        }

        return moveAcceleration;
    }

    private Vector2 SnapTo8(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;
        float rad = snapped * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
}