using UnityEngine;

public class PlayerGroundSupportProbe : MonoBehaviour
{
    [SerializeField] private Transform probeOrigin;
    [SerializeField] private float probeRadius = 0.08f;
    [SerializeField] private LayerMask groundSupportMask;
    [SerializeField] private bool drawDebugGizmos = true;

    public bool HasGroundSupport { get; private set; }

    private void Awake()
    {
        if (probeOrigin == null)
        {
            probeOrigin = transform;
        }
    }

    private void Update()
    {
        RefreshGroundSupport();
    }

    public void RefreshGroundSupport()
    {
        Collider2D supportCollider = Physics2D.OverlapCircle(
            probeOrigin.position,
            probeRadius,
            groundSupportMask);

        HasGroundSupport = supportCollider is not null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
        {
            return;
        }

        Transform currentProbeOrigin = probeOrigin != null ? probeOrigin : transform;

        Gizmos.color = HasGroundSupport ? Color.green : Color.red;
        Gizmos.DrawWireSphere(currentProbeOrigin.position, probeRadius);
    }
}