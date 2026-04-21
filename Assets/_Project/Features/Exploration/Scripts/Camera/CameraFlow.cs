using UnityEngine;

public class CameraFlow : MonoBehaviour
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
        if (target == null)
        {
            return;
        }

        Vector3 targetPos = target.position + offset;

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