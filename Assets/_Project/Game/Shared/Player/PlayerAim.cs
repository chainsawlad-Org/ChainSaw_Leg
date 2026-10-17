using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Transform aimTransform;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        UpdateAim();
    }

    private void UpdateAim()
    {
        Vector2 dir = movement.LastMoveDir;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        aimTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}