using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerFallStateOrchestrator : MonoBehaviour
{
    [SerializeField] private PlayerGroundSupportProbe groundSupportProbe;
    [SerializeField] private PlayerFallVisualPresenter playerFallVisualPresenter;
    [SerializeField] private Rigidbody2D rb;

    [Header("Fall Settings")]
    [SerializeField] private float failHeightThreshold = -3f;
    [SerializeField] private bool stopPlayModeAtThreshold = true;

    [Header("Parabolic Fall")]
    [SerializeField] private float fallAcceleration = 9.81f;
    [SerializeField] private float maxFallSpeed = 19.62f;

    [Header("Planar Inertia")]
    [SerializeField] private float planarDeceleration = 5f;
    [SerializeField] private float minimumPlanarSpeed = 0.05f;

    [Header("Occlusion Detection")]
    [SerializeField] private Vector2 hiddenSideWorldDirectionA = Vector2.left;
    [SerializeField] private Vector2 hiddenSideWorldDirectionB = Vector2.up;
    [SerializeField] [Range(-1f, 1f)] private float hiddenSideDotThreshold = 0.35f;

    public bool IsFalling { get; private set; }
    public float CurrentHeight { get; private set; }

    private bool failTriggered;
    private Vector2 currentPlanarFallVelocity;
    private float currentVerticalFallSpeed;
    private bool isOccludedByPlatform;

    private void Awake()
    {
        if (groundSupportProbe is null)
        {
            groundSupportProbe = GetComponent<PlayerGroundSupportProbe>();
        }

        if (playerFallVisualPresenter is null)
        {
            playerFallVisualPresenter = GetComponent<PlayerFallVisualPresenter>();
        }

        if (rb is null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        ResetFallState();
    }

    private void Update()
    {
        if (groundSupportProbe is null || playerFallVisualPresenter is null || rb is null || failTriggered)
        {
            return;
        }

        groundSupportProbe.RefreshGroundSupport();

        if (!IsFalling)
        {
            if (!groundSupportProbe.HasGroundSupport)
            {
                BeginFall();
            }

            return;
        }

        UpdatePlanarVelocity(Time.deltaTime);
        UpdateVerticalFall(Time.deltaTime);

        playerFallVisualPresenter.ApplyHeight(CurrentHeight);

        if (CurrentHeight <= failHeightThreshold)
        {
            HandleFailThresholdReached();
        }
    }

    public void BeginFall()
    {
        if (IsFalling)
        {
            return;
        }

        IsFalling = true;
        CurrentHeight = 0f;
        currentVerticalFallSpeed = 0f;

        currentPlanarFallVelocity = rb.linearVelocity;
        isOccludedByPlatform = ShouldBeOccludedByPlatform(currentPlanarFallVelocity);

        playerFallVisualPresenter.SetOccludedByPlatform(isOccludedByPlatform);
        playerFallVisualPresenter.ApplyHeight(CurrentHeight);
    }

    public void ResetFallState()
    {
        failTriggered = false;
        IsFalling = false;
        CurrentHeight = 0f;
        currentVerticalFallSpeed = 0f;
        currentPlanarFallVelocity = Vector2.zero;
        isOccludedByPlatform = false;

        if (playerFallVisualPresenter != null)
        {
            playerFallVisualPresenter.ResetVisual();
        }
    }

    private void UpdatePlanarVelocity(float deltaTime)
    {
        if (currentPlanarFallVelocity.sqrMagnitude > minimumPlanarSpeed * minimumPlanarSpeed)
        {
            transform.position += (Vector3)(currentPlanarFallVelocity * deltaTime);

            currentPlanarFallVelocity = Vector2.MoveTowards(
                currentPlanarFallVelocity,
                Vector2.zero,
                planarDeceleration * deltaTime);

            return;
        }

        currentPlanarFallVelocity = Vector2.zero;
    }

    private void UpdateVerticalFall(float deltaTime)
    {
        currentVerticalFallSpeed = Mathf.Min(
            currentVerticalFallSpeed + fallAcceleration * deltaTime,
            maxFallSpeed);

        CurrentHeight -= currentVerticalFallSpeed * deltaTime;
    }

    private bool ShouldBeOccludedByPlatform(Vector2 planarVelocity)
    {
        if (planarVelocity.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Vector2 direction = planarVelocity.normalized;

        Vector2 hiddenDirectionA = hiddenSideWorldDirectionA.sqrMagnitude > 0.0001f
            ? hiddenSideWorldDirectionA.normalized
            : Vector2.left;

        Vector2 hiddenDirectionB = hiddenSideWorldDirectionB.sqrMagnitude > 0.0001f
            ? hiddenSideWorldDirectionB.normalized
            : Vector2.up;

        bool matchesHiddenSideA = Vector2.Dot(direction, hiddenDirectionA) >= hiddenSideDotThreshold;
        bool matchesHiddenSideB = Vector2.Dot(direction, hiddenDirectionB) >= hiddenSideDotThreshold;

        return matchesHiddenSideA || matchesHiddenSideB;
    }

    private void HandleFailThresholdReached()
    {
        failTriggered = true;

#if UNITY_EDITOR
        if (stopPlayModeAtThreshold)
        {
            EditorApplication.isPlaying = false;
            return;
        }
#endif

        Application.Quit();
    }
}