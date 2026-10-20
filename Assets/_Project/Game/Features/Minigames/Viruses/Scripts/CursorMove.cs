using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class CursorMove : MonoBehaviour
    {
        public float speed = 2f;
        public Rigidbody2D rb;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;

        private void Awake()
        {  
            var map = inputActions.FindActionMap("Player");
            moveAction = map.FindAction("Move");
        }

        private void OnEnable() => moveAction.Enable();
        private void OnDisable() => moveAction.Disable();

        private void FixedUpdate()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            rb.linearVelocity = input * speed;
        }
    }
}
