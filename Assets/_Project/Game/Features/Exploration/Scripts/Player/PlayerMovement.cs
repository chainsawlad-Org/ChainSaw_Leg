using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool snapTo8Directions = true;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private bool flipSprite;

    private Rigidbody2D rb;
    private PlayerInputHandler inputHandler;
    private PlayerDash dash;
    private float currentSpeed;

    public Vector2 LastMoveDir { get; private set; } = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        inputHandler = GetComponent<PlayerInputHandler>();
        dash = GetComponent<PlayerDash>();
        currentSpeed = moveSpeed;
    }

    private void FixedUpdate()
    {
        Move();
        FlipSprite();
    }

    private void Move()
    {
        if (dash != null && dash.IsDashing)
            return;

        Vector2 move = inputHandler.MoveInput;

        if (move.sqrMagnitude > 1f)
            move = move.normalized;

        Vector2 moveDir = snapTo8Directions ? SnapTo8(move) : move;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            LastMoveDir = moveDir;
        }

        rb.linearVelocity = moveDir * currentSpeed;
    }

    private void FlipSprite()
    {
        if (!flipSprite) return;
        if (LastMoveDir.x > 0f && sprite.flipX) sprite.flipX = false;
        else if (LastMoveDir.x < 0f && !sprite.flipX) sprite.flipX = true;
    }

    public void SetSpeed(float value)
    {
        currentSpeed = value;
    }

    public void SetDefaultSpeed()
    {
        currentSpeed = moveSpeed;
    }

    public void TransitToPosition(Vector2 position, float duration)
    {
        transform.DOMove(position, duration).SetEase(Ease.OutQuart);
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

    public void ResetMovementState()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
