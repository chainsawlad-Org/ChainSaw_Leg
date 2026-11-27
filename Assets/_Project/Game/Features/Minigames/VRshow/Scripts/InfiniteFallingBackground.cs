using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Simulates endless downward falling: the assigned background tiles scroll
    /// upward while everything else stays in place. When a tile's bottom edge
    /// passes the top of the camera view it is teleported directly below the
    /// lowest tile, so two or more stacked tiles loop seamlessly forever. Use
    /// two or more equally sized tiles stacked vertically for a continuous look.
    /// </summary>
    public sealed class InfiniteFallingBackground : MonoBehaviour
    {
        [Tooltip("How fast the background moves up, in world units per second.")]
        [SerializeField, Min(0f)] private float scrollSpeed = 3f;
        [Tooltip("Background tiles to scroll. All tiles should share the same height.")]
        [SerializeField] private List<Transform> tiles = new List<Transform>();
        [Tooltip("Extra distance above the camera top a tile must pass before wrapping.")]
        [SerializeField, Min(0f)] private float wrapMargin = 1f;
        [Tooltip("Camera the wrap is computed against. Falls back to Camera.main.")]
        [SerializeField] private Camera viewCamera;

        private readonly List<float> tileHeights = new();
        private Camera cachedCamera;

        private void Update()
        {
            if (tiles == null || tiles.Count == 0) return;

            Camera camera = GetCamera();
            if (camera == null) return;

            float delta = scrollSpeed * Time.deltaTime;
            float cameraTop = camera.ViewportToWorldPoint(Vector3.one).y;

            for (int i = 0; i < tiles.Count; i++)
            {
                Transform tile = tiles[i];
                if (tile == null) continue;

                tile.position += Vector3.up * delta;

                float height = GetTileHeight(i);
                if (height <= 0f) continue;

                // Wrap once the tile is fully above the view: teleport it below
                // the lowest tile so the stack never shows a gap.
                float bottom = tile.position.y - height * 0.5f;
                if (bottom > cameraTop + wrapMargin)
                {
                    float lowestBottom = GetLowestBottom();
                    tile.position = new Vector3(tile.position.x, lowestBottom - height * 0.5f, tile.position.z);
                }
            }
        }

        private float GetTileHeight(int index)
        {
            while (tileHeights.Count <= index) tileHeights.Add(0f);

            if (tileHeights[index] > 0f) return tileHeights[index];

            Transform tile = tiles[index];
            if (tile == null) return 0f;

            SpriteRenderer renderer = tile.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"InfiniteFallingBackground: tile '{tile.name}' has no SpriteRenderer — it cannot be wrapped.", tile);
                return 0f;
            }

            float height = renderer.bounds.size.y;
            tileHeights[index] = height;

            return height;
        }

        private float GetLowestBottom()
        {
            float lowest = float.PositiveInfinity;

            for (int i = 0; i < tiles.Count; i++)
            {
                Transform tile = tiles[i];
                if (tile == null) continue;

                float height = GetTileHeight(i);
                float bottom = height > 0f ? tile.position.y - height * 0.5f : tile.position.y;

                if (bottom < lowest) lowest = bottom;
            }

            return float.IsPositiveInfinity(lowest) ? 0f : lowest;
        }

        private Camera GetCamera()
        {
            if (cachedCamera == null) cachedCamera = viewCamera != null ? viewCamera : Camera.main;

            if (cachedCamera == null)
                Debug.LogWarning("InfiniteFallingBackground: no viewCamera assigned and no Camera.main found — the background will not wrap.", this);

            return cachedCamera;
        }
    }
}
