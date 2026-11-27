using ChainSawLeg.Features.Exploration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Free-flight player for the FlyEvilMisty boss fight. Moves on X and Y with a
    /// light inertia (Rigidbody2D with gravity disabled) driven by the hardcoded
    /// WASD/arrow keys — same input style as PlatformerPlayerMovement, no input
    /// actions — and is kept inside the static camera view with a small padding.
    /// While this player is active the scene camera is parked at the arena anchor
    /// and the main player's CameraFlow is disabled; everything is restored when
    /// the object is disabled again. External velocity (dash, knockback) suspends
    /// control briefly via <see cref="SuspendControl"/> so it is not overwritten
    /// by the input; damage knockback comes from PlatformerPlayerHealth.
    /// </summary>
    public sealed class FlyEvilMistyPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.01f)] private float moveSpeed = 10f;
        [Tooltip("How quickly the velocity approaches the input direction. Higher = heavier inertia.")]
        [SerializeField, Min(0.01f)] private float moveSmoothTime = 0.08f;

        [Header("Screen Bounds")]
        [Tooltip("Extra distance kept from the screen edges, on top of the collider extents.")]
        [SerializeField, Min(0f)] private Vector2 padding = new Vector2(0.2f, 0.2f);

        [Header("Spawn")]
        [Tooltip("Position the flight starts at, in world units. Only applied when Reset To Start On Enable is on.")]
        [SerializeField] private Vector2 startPosition = new Vector2(0f, 6f);
        [Tooltip("Snap to Start Position on enable. Leave off when the arena places this player at its spawn point itself.")]
        [SerializeField] private bool resetToStartOnEnable;

        [Header("Damage")]
        [Tooltip("How long the input hands the velocity over to the hit knockback.")]
        [SerializeField, Min(0f)] private float damageControlSuspendTime = 0.35f;

        [Header("Camera")]
        [Tooltip("CameraFlow that follows the main platformer player. Disabled while this player flies.")]
        [SerializeField] private CameraFlow cameraFlow;
        [Tooltip("Anchor the camera is parked at while the FlyEvilMisty player is active.")]
        [SerializeField] private Transform arenaCameraAnchor;
        [Tooltip("Camera used for the screen bounds. Falls back to Camera.main.")]
        [SerializeField] private Camera arenaCamera;

        [Header("References")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private PlatformerPlayerHealth health;

        private Vector2 velocitySmoothed;
        private float controlSuspendedUntil = -Mathf.Infinity;
        private Vector3 cameraRigRestorePosition;
        private bool cameraRigParked;

        /// <summary>True while a dash/knockback owns the velocity and input is ignored.</summary>
        public bool IsControlSuspended => Time.time < controlSuspendedUntil;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            if (health == null) health = GetComponent<PlatformerPlayerHealth>();

            if (cameraFlow == null) cameraFlow = FindFirstObjectByType<CameraFlow>();
            if (cameraFlow == null)
                Debug.LogWarning("FlyEvilMistyPlayerController: no CameraFlow found — the camera will not be swapped to the arena.", this);

            if (health != null) health.Damaged += OnDamaged;
        }

        private void OnDestroy()
        {
            if (health != null) health.Damaged -= OnDamaged;
        }

        private void OnEnable()
        {
            arenaCamera = arenaCamera != null ? arenaCamera : Camera.main;

            if (resetToStartOnEnable) MoveTo(startPosition);

            if (body != null) body.linearVelocity = Vector2.zero;
            velocitySmoothed = Vector2.zero;
            controlSuspendedUntil = -Mathf.Infinity;

            ParkCameraToArena();
        }

        private void OnDisable()
        {
            RestoreCamera();
        }

        private void FixedUpdate()
        {
            if (body == null || IsControlSuspended) return;

            Vector2 input = ReadMoveInput();
            body.linearVelocity = Vector2.SmoothDamp(body.linearVelocity, input * moveSpeed, ref velocitySmoothed, moveSmoothTime);

            ClampToScreen();
        }

        /// <summary>
        /// Briefly hands the velocity over to an external system (dash, knockback):
        /// the controller neither writes velocity nor clamps to the screen bounds
        /// until the duration elapses, so the impulse plays out untouched.
        /// </summary>
        public void SuspendControl(float duration)
        {
            if (duration <= 0f) return;

            controlSuspendedUntil = Mathf.Max(controlSuspendedUntil, Time.time + duration);
        }

        private void OnDamaged(int currentHealth, int maxHealth)
        {
            // PlatformerPlayerHealth already pushed the body away from the hit;
            // let the knockback play out before the input takes over again.
            SuspendControl(damageControlSuspendTime);
        }

        private Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            Vector2 input = Vector2.zero;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;

            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void ClampToScreen()
        {
            if (arenaCamera == null) return;

            Vector3 min = arenaCamera.ViewportToWorldPoint(Vector3.zero);
            Vector3 max = arenaCamera.ViewportToWorldPoint(Vector3.one);

            Vector2 extents = GetHalfSize();
            float minX = min.x + padding.x + extents.x;
            float maxX = max.x - padding.x - extents.x;
            float minY = min.y + padding.y + extents.y;
            float maxY = max.y - padding.y - extents.y;

            Vector2 position = body.position;
            Vector2 clamped = new Vector2(Mathf.Clamp(position.x, minX, maxX), Mathf.Clamp(position.y, minY, maxY));

            if (clamped == position) return;

            body.position = clamped;
            transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);

            // Cancel only the outward velocity so the player can slide along the edge.
            Vector2 velocity = body.linearVelocity;
            if (clamped.x != position.x) velocity.x = 0f;
            if (clamped.y != position.y) velocity.y = 0f;
            body.linearVelocity = velocity;
        }

        private Vector2 GetHalfSize()
        {
            if (bodyCollider == null) return padding;

            Vector2 extents = bodyCollider.bounds.extents;

            return extents + padding;
        }

        private void MoveTo(Vector2 position)
        {
            if (body != null) body.position = position;

            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void ParkCameraToArena()
        {
            if (cameraRigParked || cameraFlow == null) return;

            cameraRigRestorePosition = cameraFlow.transform.position;
            cameraFlow.enabled = false;
            cameraRigParked = true;

            if (arenaCameraAnchor != null)
                cameraFlow.transform.position = new Vector3(
                    arenaCameraAnchor.position.x,
                    arenaCameraAnchor.position.y,
                    cameraRigRestorePosition.z);
        }

        private void RestoreCamera()
        {
            if (!cameraRigParked) return;

            cameraRigParked = false;

            if (cameraFlow == null) return;

            cameraFlow.transform.position = cameraRigRestorePosition;
            cameraFlow.enabled = true;
        }
    }
}
