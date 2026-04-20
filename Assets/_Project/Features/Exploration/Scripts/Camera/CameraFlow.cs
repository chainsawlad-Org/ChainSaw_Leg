using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZoneRadius = 1.5f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position + offset;

        float distance = Vector2.Distance(currentPos, targetPos);

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