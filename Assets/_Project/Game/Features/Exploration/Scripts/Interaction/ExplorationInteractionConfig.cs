using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_ExplorationInteractionConfig",
    menuName = "ChainSawLeg/Exploration/Interaction Config")]
public class ExplorationInteractionConfig : ScriptableObject
{
    private const int DefaultInteractionLayerIndex = 6;

    [Header("Detection")]
    [SerializeField] private float interactionRadius = 1.75f;
    [SerializeField] private float scanIntervalSeconds = 0.05f;
    [SerializeField] private LayerMask interactionLayerMask = 1 << DefaultInteractionLayerIndex;

    [Header("Direction")]
    [SerializeField] [Range(0f, 180f)] private float directPriorityHalfAngleDegrees = 22.5f;
    [SerializeField] [Range(0f, 180f)] private float interactionHalfAngleDegrees = 45f;

    [Header("Presentation")]
    [SerializeField] private string defaultPromptText = "Press E to interact";

    public float InteractionRadius => interactionRadius;
    public float ScanIntervalSeconds => scanIntervalSeconds;
    public LayerMask InteractionLayerMask => interactionLayerMask;
    public float DirectPriorityHalfAngleDegrees => directPriorityHalfAngleDegrees;
    public float InteractionHalfAngleDegrees => interactionHalfAngleDegrees;
    public string DefaultPromptText => defaultPromptText;
}
