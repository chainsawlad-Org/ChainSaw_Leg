using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float smoothTime = 0.15f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZoneRadius = 1.5f;

    private Vector3 offset;
    private Vector3 velocity;

    private void Start()
    {
        if (target == null) return;
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position + offset;

        float distance = Vector3.Distance(currentPos, targetPos);

        if (distance < deadZoneRadius)
            return;

        transform.position = Vector3.SmoothDamp(
            currentPos,
            targetPos,
            ref velocity,
            smoothTime
        );
    }
}