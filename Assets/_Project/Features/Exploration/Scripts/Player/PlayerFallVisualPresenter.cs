using UnityEngine;
using UnityEngine.Rendering;

public class PlayerFallVisualPresenter : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SortingGroup visualSortingGroup;
    [SerializeField] private Vector2 visualFallDirection = new Vector2(0f, -1f);
    [SerializeField] private float visualUnitsPerHeightUnit = 1f;
    [SerializeField] private float minimumScale = 0.9f;
    [SerializeField] private float scalePerHeightUnit = 0.03f;

    [Header("Sorting")]
    [SerializeField] private string normalSortingLayerName = "Characters";
    [SerializeField] private int normalOrderInLayer = 0;
    [SerializeField] private string occludedSortingLayerName = "Ground";
    [SerializeField] private int occludedOrderInLayer = -1;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        if (visualSortingGroup == null)
        {
            visualSortingGroup = visualRoot.GetComponent<SortingGroup>();
        }

        baseLocalPosition = visualRoot.localPosition;
        baseLocalScale = visualRoot.localScale;

        ApplyNormalSorting();
    }

    public void ApplyHeight(float currentHeight)
    {
        if (visualRoot == null)
        {
            return;
        }

        float fallDistance = Mathf.Max(0f, -currentHeight);

        Vector2 normalizedDirection = visualFallDirection.sqrMagnitude > 0.0001f
            ? visualFallDirection.normalized
            : Vector2.down;

        Vector3 offset = new Vector3(
            normalizedDirection.x,
            normalizedDirection.y,
            0f) * (fallDistance * visualUnitsPerHeightUnit);

        visualRoot.localPosition = baseLocalPosition + offset;

        float visualScale = Mathf.Max(
            minimumScale,
            1f - fallDistance * scalePerHeightUnit);

        visualRoot.localScale = baseLocalScale * visualScale;
    }

    public void SetOccludedByPlatform(bool isOccludedByPlatform)
    {
        if (visualSortingGroup == null)
        {
            return;
        }

        if (isOccludedByPlatform)
        {
            visualSortingGroup.sortingLayerName = occludedSortingLayerName;
            visualSortingGroup.sortingOrder = occludedOrderInLayer;
            return;
        }

        ApplyNormalSorting();
    }

    public void ResetVisual()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition = baseLocalPosition;
        visualRoot.localScale = baseLocalScale;
        ApplyNormalSorting();
    }

    private void ApplyNormalSorting()
    {
        if (visualSortingGroup == null)
        {
            return;
        }

        visualSortingGroup.sortingLayerName = normalSortingLayerName;
        visualSortingGroup.sortingOrder = normalOrderInLayer;
    }
}