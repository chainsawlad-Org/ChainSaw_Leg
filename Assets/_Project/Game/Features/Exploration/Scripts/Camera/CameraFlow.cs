using DG.Tweening;
using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The player object to follow")]
    public Transform player;
    
    [Header("Movement Settings")]
    [Tooltip("How smoothly the camera follows the player")]
    [Range(0.1f, 1f)]
    public float smoothSpeed = 0.125f;
    
    [Tooltip("Offset from the player position")]
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Tooltip("Whether to follow on X axis")]
    public bool followX = true;
    
    [Tooltip("Whether to follow on Y axis")]
    public bool followY = true;
    
    [Tooltip("Whether to follow on Z axis")]
    public bool followZ = true;

    public Bounds bounds;
    
    private Vector3 velocity = Vector3.zero;
    private bool inTransition;
    
    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Player not found! Please assign the player transform.");
        }
        
        if (player != null)
            transform.position = player.position + offset;
    }

    public void TransitToRoom(Bounds bounds, float duration)
    {
        inTransition = true;
        velocity = Vector3.zero;
        Vector3 target = GetClampedPosition(GetDesiredPosition(player.position), bounds);
        transform.DOMove(target, duration).SetEase(Ease.InOutQuart).OnComplete(() => inTransition = false);
    }
    
    private void LateUpdate()
    {
        if (player == null) return;
        if (inTransition) return;
        
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            GetDesiredPosition(transform.position),
            ref velocity,
            smoothSpeed
        );
        
        transform.position = GetClampedPosition(smoothedPosition, bounds);
    }

    private Vector3 GetDesiredPosition(Vector3 position)
    {
        Vector3 desiredPosition = position;
        
        if (followX)
            desiredPosition.x = player.position.x + offset.x;
        if (followY)
            desiredPosition.y = player.position.y + offset.y;
        if (followZ)
            desiredPosition.z = player.position.z + offset.z;
        
        return desiredPosition;
    }
    
    private Vector3 GetClampedPosition(Vector3 position, Bounds bounds)
    {
        if (!TryGetComponent(out Camera cam)) return position;
        
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        if (bounds.size.x <= horzExtent && bounds.size.y <= vertExtent)
        {
            position = new Vector3(bounds.center.x, bounds.center.y, position.z);
            return position;
        }
            
        position.x = Mathf.Clamp(
            position.x,
            bounds.min.x + horzExtent,
            bounds.max.x - horzExtent
        );
        position.y = Mathf.Clamp(
            position.y,
            bounds.min.y + vertExtent,
            bounds.max.y - vertExtent
        );
        
        if (bounds.size.x <= horzExtent * 2) position.x = bounds.center.x;
        if (bounds.size.y <= vertExtent * 2) position.y = bounds.center.y;
        
        return position;
    }
}