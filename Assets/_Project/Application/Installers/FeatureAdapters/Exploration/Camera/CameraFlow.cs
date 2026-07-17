using ChainSawLeg.Features.Exploration.Save;
using UnityEngine;
using Zenject;

public class CameraFlow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.1f;

    private Vector3 offset;
    private Vector3 velocity;
    private ExplorationPlayerRegistry playerRegistry;
    private bool offsetInitialized;

    public Transform Target => target;
    public float SmoothTime => smoothTime;
    public Vector3 FollowVelocity => velocity;
    public Vector3 DesiredPosition =>
        target != null && offsetInitialized
            ? target.position + offset
            : transform.position;
    public float FollowError => Vector3.Distance(transform.position, DesiredPosition);

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
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (target == null || !offsetInitialized)
            return;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            DesiredPosition,
            ref velocity,
            smoothTime);
    }
}
