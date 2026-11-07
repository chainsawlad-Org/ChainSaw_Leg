using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class PlatformerPlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float jumpForce = 50f;
        [SerializeField] private SpriteRenderer sprite;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
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
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                rb.linearVelocityY = jumpForce;
            }
        }
    }
}
