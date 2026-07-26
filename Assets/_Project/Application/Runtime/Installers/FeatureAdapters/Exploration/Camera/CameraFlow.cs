using ChainSawLeg.Features.Exploration.Save;
using UnityEngine;
using Zenject;

public class CameraFlow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZoneRadius = 1.5f;

    private Vector3 offset;
    private Vector3 velocity;
    private Vector3 previousTargetPosition;
    private ExplorationPlayerRegistry playerRegistry;
    private bool offsetInitialized;
    private bool targetPositionInitialized;

    private void Awake()
    {
        CaptureOffset();
    }

    [Inject]
    public void Construct(ExplorationPlayerRegistry playerRegistry)
    {
        this.playerRegistry = playerRegistry;
        playerRegistry.PositionRestored += SnapToTarget;
    }

    private void Start()
    {
        if (!offsetInitialized)
            CaptureOffset();

        SnapToTarget();
    }

    private void OnDestroy()
    {
        if (playerRegistry != null)
            playerRegistry.PositionRestored -= SnapToTarget;
    }

    private void CaptureOffset()
    {
        if (target == null)
            return;

        offset = transform.position - target.position;
        offsetInitialized = true;
    }

    private void SnapToTarget()
    {
        if (target == null || !offsetInitialized)
            return;

        velocity = Vector3.zero;
        transform.position = target.position + offset;
        previousTargetPosition = target.position;
        targetPositionInitialized = true;
    }

    private void LateUpdate()
    {

        if (!Application.isPlaying)
            return;

        if (target == null)
            return;

        Vector3 currentTargetPosition = target.position;

        if (targetPositionInitialized &&
            Vector3.Distance(previousTargetPosition, currentTargetPosition) >= deadZoneRadius)
        {
            SnapToTarget();
            return;
        }

        previousTargetPosition = currentTargetPosition;
        targetPositionInitialized = true;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentTargetPosition + offset;

        float distance = Vector3.Distance(currentPos, targetPos);

        if (distance < deadZoneRadius)
            return;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime);
    }
}
