using System;
using UnityEngine;

namespace ChainSawLeg.Game.Shared
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSorter : MonoBehaviour
    {
        [SerializeField] private int precisionMultiplier = 100;
        [Header("Sort transform")]
        [SerializeField] private bool useSortTransform;
        [SerializeField] private Transform sortTransform;
        [Header("Offset")]
        [SerializeField] private Vector2 offset;
        [Header("Sprite below")]
        [SerializeField] private bool setAnotherBelowSprite;
        [SerializeField] private SpriteRenderer spriteBelow;
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos;
        
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void LateUpdate()
        {
            if (setAnotherBelowSprite) spriteRenderer.sortingOrder = spriteBelow.sortingOrder + 1;
            else
            {
                Transform point = useSortTransform ? sortTransform : transform;
                Vector2 position = (Vector2)point.position + offset;
                spriteRenderer.sortingOrder = Mathf.RoundToInt(position.y * precisionMultiplier * -1);
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (setAnotherBelowSprite) return;
            
            Transform point = useSortTransform ? sortTransform : transform;
            Vector2 position = (Vector2)point.position + offset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, 0.25f);
        }
    }
}
