using NUnit.Framework;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private PlayerFallStateOrchestrator playerFallStateOrchestrator;

    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerMovement movement;
    private float dashTimer;
    private float cooldownTimer;
    private Vector2 dashDirection;

    public bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();

        if (playerFallStateOrchestrator == null)
        {
            playerFallStateOrchestrator = GetComponent<PlayerFallStateOrchestrator>();
        }
    }

    private void Update()
    {
        if (playerFallStateOrchestrator != null && playerFallStateOrchestrator.IsFalling)
        {
            CancelDash();
            return;
        }

        HandleDashInput();
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        if (playerFallStateOrchestrator != null && playerFallStateOrchestrator.IsFalling)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashForce;
        }
    }

    private void HandleDashInput()
    {
        if (!input.DashPressed)
        {
            return;
        }

        input.ConsumeDash();

        if (cooldownTimer > 0f)
        {
            return;
        }

        StartDash();
    }

    private void StartDash()
    {
        dashDirection = movement.LastMoveDir;

        if (dashDirection.sqrMagnitude < 0.001f)
        {
            dashDirection = Vector2.up;
        }

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
    }

    private void UpdateTimers()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (!isDashing)
        {
            return;
        }

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }

    private void CancelDash()
    {
        isDashing = false;
        dashTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}