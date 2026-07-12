using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_ExplorationInteractionConfig",
    menuName = "ChainSawLeg/Exploration/Interaction Config")]
public class ExplorationInteractionConfig : ScriptableObject
{
    [Header("Detection")]
    [SerializeField] private float interactionRadius = 1.75f;
    [SerializeField] private float scanIntervalSeconds = 0.05f;
    [SerializeField] private LayerMask interactionLayerMask = ~0;

    [Header("Facing")]
    [SerializeField] [Range(-1f, 1f)] private float facingDotThreshold = 0.7f;

    [Header("Presentation")]
    [SerializeField] private string defaultPromptText = "Press E to interact";

    public float InteractionRadius => interactionRadius;
    public float ScanIntervalSeconds => scanIntervalSeconds;
    public LayerMask InteractionLayerMask => interactionLayerMask;
    public float FacingDotThreshold => facingDotThreshold;
    public string DefaultPromptText => defaultPromptText;
}