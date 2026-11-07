using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float jumpForce = 50f;
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

        private void Update()
        {
            ProcessMove();
            ProcessJump();
        }

        private void ProcessMove()
        {
            float xVelocity = 0f;

            if (Keyboard.current.aKey.isPressed) xVelocity = -1f;
            if (Keyboard.current.dKey.isPressed) xVelocity = 1f;
            
            rb.linearVelocityX = xVelocity * speed;
            
            if (xVelocity > 0f) sprite.flipX = false;
            else if (xVelocity < 0f) sprite.flipX = true;
        }

        private void ProcessJump()
        {
            if (Keyboard.current.wKey.wasPressedThisFrame && IsGrounded())
            {
                rb.linearVelocityY = jumpForce;
            }
        }

        private bool IsGrounded()
        {
            Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.01f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundMask);
            Debug.DrawRay(origin, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);

            return hit.collider != null;
        }
    }
}
