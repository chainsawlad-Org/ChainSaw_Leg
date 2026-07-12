// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PlayerController : MonoBehaviour
// {
//     private PlayerInputActions input;
//     private Rigidbody2D rb;

//     [SerializeField] private Transform aimTransform;

//     // [Header("Movement Settings")]
//     // [SerializeField] private float moveSpeed = 10f;

//     // [Header("8 Direction Movement")]
//     // [SerializeField] private bool snapTo8Directions = true;

//     // private Vector2 moveInput = Vector2.zero;
//     private Vector2 lastMoveDir = Vector2.up;

//     private void Awake()
//     {
//         input = new PlayerInputActions();
//         rb = GetComponent<Rigidbody2D>();
//     }


//     private void Update()
//     {
//         UpdateAim();
//     }




//     private void UpdateAim()
//     {
//         if (aimTransform == null) return;

//         Vector2 dir = lastMoveDir;

//         float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

//         aimTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
//     }
// }