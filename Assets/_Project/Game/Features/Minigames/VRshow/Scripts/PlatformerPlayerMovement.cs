using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float jumpForce = 50f;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private SpriteRenderer sprite;
        private Rigidbody2D rb;
        private Collider2D col;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        private bool canMove = true;
        private bool launchedUp;
        private float lastGroundedTime = -Mathf.Infinity;
        private float lastJumpPressedTime = -Mathf.Infinity;

        public void LockMovement() => canMove = false;

        public void UnlockMovement() => canMove = true;

        public bool IsGrounded()
        {
            Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.01f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundMask);
            Debug.DrawRay(origin, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);

            return hit.collider != null;
        }

        public void LaunchUp(float force)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            launchedUp = true;
        }

        private void Update()
        {
            ProcessMove();
            ProcessJump();
        }

        private void ProcessMove()
        {
            if (!canMove)
            {
                rb.linearVelocityX = 0f;
                return;
            }

            float xVelocity = 0f;

            if (Keyboard.current.leftArrowKey.isPressed) xVelocity = -1f;
            if (Keyboard.current.rightArrowKey.isPressed) xVelocity = 1f;
            
            rb.linearVelocityX = xVelocity * speed;
            
            if (xVelocity > 0f) sprite.flipX = false;
            else if (xVelocity < 0f) sprite.flipX = true;
        }

        private void ProcessJump()
        {
            bool isGrounded = IsGrounded();
            if (isGrounded) lastGroundedTime = Time.time;

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                lastJumpPressedTime = Time.time;
            bool coyoteAvailable = Time.time - lastGroundedTime <= coyoteTime;
            bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;

            if (canMove && jumpBuffered && coyoteAvailable)
            {
                rb.linearVelocityY = jumpForce;
                lastGroundedTime = -Mathf.Infinity;
                lastJumpPressedTime = -Mathf.Infinity;
            }

            if (!launchedUp
                && (Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.upArrowKey.wasReleasedThisFrame)
                && rb.linearVelocityY > 0f)
            {
                rb.linearVelocityY *= jumpCutMultiplier;
            }

            if (isGrounded) launchedUp = false;
        }

    }
}
