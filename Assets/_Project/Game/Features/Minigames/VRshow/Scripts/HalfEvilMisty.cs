using System.Collections;
using System.Collections.Generic;
using ChainSawLeg.Features.Exploration;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Half Evil Misty boss: once in a while performs a Mad Dummy-style triangle
    /// volley. Yellow warning triangles first appear on the UI at the bottom of
    /// the screen, marking the columns where triangles will fly; after the
    /// warning time they disappear and a row of triangles spawns on the boss
    /// and flies upward. The warning phase plus the spacing between triangles
    /// is the dodge window: the player can leave the columns or slip between them.
    /// </summary>
    public sealed class HalfEvilMisty : MonoBehaviour
    {
        [Header("Attack Loop")]
        [SerializeField, Min(0f)] private float firstAttackDelay = 2f;
        [SerializeField, Min(0f)] private float attackInterval = 4f;

        [Header("Triangle Volley")]
        [SerializeField] private GameObject trianglePrefab;
        [SerializeField, Min(1)] private int triangleCount = 10;
        [Tooltip("How wide the safe gap in the triangle wall is, in world units. Rounded up to whole columns (one column = triangleSpacing wide).")]
        [SerializeField, Min(0f)] private float holeWidth = 2.2f;
        [SerializeField, Min(0f)] private float triangleSpacing = 2.2f;
        [SerializeField, Min(0.01f)] private float triangleSpeed = 8f;
        [SerializeField, Min(0.01f)] private float triangleLifetime = 4f;
        [SerializeField, Min(1)] private int triangleDamage = 1;
        [SerializeField, Min(0f)] private float triangleDamageRadius = 0.5f;
        [Tooltip("Triangles spawn at the boss's height, but never further below the player than this, so a player high above the boss doesn't wait for the attack.")]
        [SerializeField, Min(0f)] private float maxSpawnHeightBelowPlayer = 5f;

        [Header("Warning UI")]
        [SerializeField] private Canvas warningCanvas;
        [SerializeField, Min(0f)] private float warningTime = 1.5f;
        [SerializeField, Min(0f)] private float warningYOffset = 60f;
        [SerializeField, Min(1f)] private float warningSize = 36f;
        [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.25f, 1f);

        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;
        [SerializeField] private CameraFlow cameraFlow;

        private PlatformerPlayerHealth playerHealth;
        private Camera warningCamera;

        private void Awake()
        {
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            if (monsterHealth != null) monsterHealth.Died += OnBossDied;
            if (cameraFlow == null) cameraFlow = FindFirstObjectByType<CameraFlow>();
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();
            warningCamera = cameraFlow != null ? cameraFlow.camera : Camera.main;
        }

        public void StartAttack()
        {
            StartCoroutine(RunAttackLoop());
        }

        private void OnDestroy()
        {
            if (monsterHealth != null) monsterHealth.Died -= OnBossDied;

            StopAllCoroutines();
        }

        private bool BossAlive()
        {
            return monsterHealth == null || monsterHealth.IsAlive;
        }

        private void OnBossDied()
        {
            StopAllCoroutines();
        }

        private IEnumerator RunAttackLoop()
        {
            yield return new WaitForSeconds(firstAttackDelay);

            while (BossAlive())
            {
                if (playerHealth != null && playerHealth.IsAlive)
                    yield return TriangleVolley();

                yield return new WaitForSeconds(attackInterval);
            }
        }

        private IEnumerator TriangleVolley()
        {
            // Wall centered on the player's X at volley start (locked — no homing).
            float centerX = playerHealth != null ? playerHealth.transform.position.x : transform.position.x;
            float rowWidth = (triangleCount - 1) * triangleSpacing;
            float startX = centerX - rowWidth * 0.5f;

            var columns = new List<float>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
                columns.Add(startX + i * triangleSpacing);

            // Poke holes in the wall: those columns get no triangle AND no warning,
            // so the blinking row shows exactly where the safe gap is. The hole is
            // one contiguous run of columns, sized by holeWidth in world units.
            var holes = new HashSet<int>();
            int widthColumns = Mathf.Clamp(
                Mathf.CeilToInt(holeWidth / Mathf.Max(0.001f, triangleSpacing)), 0, triangleCount);
            if (widthColumns > 0)
            {
                int holeStart = Random.Range(0, triangleCount - widthColumns + 1);
                for (int i = 0; i < widthColumns; i++)
                    holes.Add(holeStart + i);
            }

            // Columns that will actually fire.
            var firingColumns = new List<float>(columns.Count);
            for (int i = 0; i < columns.Count; i++)
                if (!holes.Contains(i))
                    firingColumns.Add(columns[i]);

            // Spawn height: the boss's own level, but if the player climbed far
            // above the boss, triangles spawn closer to them so they don't have
            // to wait for the wall to travel up. Never spawns above the player
            // (triangles fly up, so they must start below to be a threat).
            float spawnY = transform.position.y;
            if (playerHealth != null && maxSpawnHeightBelowPlayer > 0f)
            {
                float playerY = playerHealth.transform.position.y;
                spawnY = Mathf.Clamp(playerY - maxSpawnHeightBelowPlayer, transform.position.y, playerY);
            }

            // --- Warning phase: UI triangles mark the firing columns at the screen bottom ---
            List<Image> warnings = null;
            Canvas canvas = warningCanvas != null ? warningCanvas : FindFirstObjectByType<Canvas>();

            if (canvas != null && warningCamera != null && warningTime > 0f)
            {
                warnings = CreateWarnings(canvas, firingColumns, spawnY);

                float elapsed = 0f;
                while (elapsed < warningTime)
                {
                    elapsed += Time.deltaTime;
                    UpdateWarnings(warnings, firingColumns, spawnY, canvas);
                    yield return null;
                }

                DestroyWarnings(warnings);
            }
            else
            {
                // No UI available: still give the same dodge window in time.
                yield return new WaitForSeconds(warningTime);
            }

            // --- Volley: triangles spawn and fly upward ---
            if (trianglePrefab == null)
            {
                Debug.LogWarning("HalfEvilMisty: no trianglePrefab assigned — volley skipped.", this);
                yield break;
            }

            foreach (float column in firingColumns)
            {
                var missileGO = Instantiate(trianglePrefab, new Vector3(column, spawnY, transform.position.z), Quaternion.identity);

                var missile = missileGO.GetComponent<BossTriangleMissile>();
                if (missile != null)
                    missile.Configure(triangleSpeed, triangleDamage, triangleDamageRadius, triangleLifetime);
            }
        }

        private List<Image> CreateWarnings(Canvas canvas, List<float> columns, float spawnY)
        {
            var warnings = new List<Image>(columns.Count);

            foreach (float column in columns)
            {
                var imageGO = new GameObject("TriangleWarning", typeof(RectTransform));
                imageGO.transform.SetParent(canvas.transform, false);

                Image image = imageGO.AddComponent<Image>();
                image.sprite = BossTriangleMissile.CreateTriangleSprite();
                image.raycastTarget = false;

                RectTransform rect = imageGO.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(warningSize, warningSize);

                warnings.Add(image);
            }

            UpdateWarnings(warnings, columns, spawnY, canvas);

            return warnings;
        }

        private void UpdateWarnings(List<Image> warnings, List<float> columns, float spawnY, Canvas canvas)
        {
            if (warnings == null || warnings.Count != columns.Count) return;

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float scale = canvasRect.lossyScale.x;

            for (int i = 0; i < warnings.Count; i++)
            {
                Vector3 screen = warningCamera.WorldToScreenPoint(new Vector3(columns[i], spawnY, 0f));
                warnings[i].rectTransform.anchoredPosition =
                    (new Vector2(screen.x, warningYOffset) - (Vector2)canvasRect.position) / scale;

                // Blink so the warnings read as "incoming".
                float alpha = Mathf.Lerp(0.35f, 1f, Mathf.PingPong(Time.time * 3f, 1f));
                warnings[i].color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            }
        }

        private static void DestroyWarnings(List<Image> warnings)
        {
            if (warnings == null) return;

            foreach (Image warning in warnings)
                if (warning != null)
                    Destroy(warning.gameObject);
        }
    }
}
