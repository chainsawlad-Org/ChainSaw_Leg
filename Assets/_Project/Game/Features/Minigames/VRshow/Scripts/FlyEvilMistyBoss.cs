using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Fly Evil Misty boss: hovers in the lower part of the screen and slowly drifts
    /// left-right, flipping direction on a random timer and at the screen edges.
    /// Every attack releases a fan of three triangles aimed at the flying player,
    /// reusing the volley and telegraph pattern of <see cref="HalfEvilMisty"/>. The
    /// boss only attacks while the dedicated FlyEvilMisty player is active. Killing
    /// it plays the usual MonsterHealth death effect and stops the attacks; it does
    /// not progress to any further room.
    /// </summary>
    public sealed class FlyEvilMistyBoss : MonoBehaviour
    {
        [Header("Patrol")]
        [Tooltip("Horizontal drift speed while patrolling the lower screen.")]
        [SerializeField, Min(0.01f)] private float horizontalSpeed = 2.5f;
        [Tooltip("How far the boss may stray from its anchor on each side, in world units.")]
        [SerializeField, Min(0f)] private float horizontalRange = 2.5f;
        [Tooltip("Average time between random direction flips.")]
        [SerializeField, Min(0.05f)] private float directionChangeInterval = 3f;
        [Tooltip("Random +/- range applied to the direction change interval.")]
        [SerializeField, Min(0f)] private float directionChangeIntervalVariation = 1.5f;
        [Tooltip("X the boss patrols around. Falls back to the boss's own start position.")]
        [SerializeField] private Transform patrolAnchor;

        [Header("Attack Loop")]
        [SerializeField, Min(0f)] private float firstAttackDelay = 2f;
        [SerializeField, Min(0f)] private float attackInterval = 3f;
        [Tooltip("Only attack while the FlyEvilMisty player object is active.")]
        [SerializeField] private bool onlyAttackWhilePlayerActive = true;

        [Header("Telegraph")]
        [Tooltip("How long the boss shakes in place before releasing the triangles.")]
        [SerializeField, Min(0f)] private float telegraphDuration = 0.5f;
        [SerializeField, Min(0f)] private float telegraphShakeStrength = 0.3f;
        [SerializeField, Min(0)] private int telegraphShakeVibrato = 40;

        [Header("Triangle Volley")]
        [SerializeField] private GameObject trianglePrefab;
        [Tooltip("Angle between the aimed triangle and each of the two side triangles.")]
        [SerializeField, Range(1f, 89f)] private float spreadAngle = 45f;
        [Tooltip("Delay between each of the three triangles leaving the boss body.")]
        [SerializeField, Min(0f)] private float triangleSpawnInterval = 0.35f;
        [SerializeField, Min(0.01f)] private float triangleSpeed = 8f;
        [SerializeField, Min(0.01f)] private float triangleLifetime = 4f;
        [SerializeField, Min(1)] private int triangleDamage = 1;
        [SerializeField, Min(0f)] private float triangleDamageRadius = 0.5f;
        [Tooltip("Offset from the boss pivot where the triangles appear.")]
        [SerializeField] private Vector2 spawnOffset;

        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;
        [Tooltip("The dedicated FlyEvilMisty player. Triangles aim at it and the boss only attacks while it is active.")]
        [SerializeField] private Transform attackTarget;
        [Tooltip("The FlyEvilMisty player's health. When assigned, the boss stops attacking while the player is dead.")]
        [SerializeField] private PlatformerPlayerHealth attackTargetHealth;
        [SerializeField] private Rigidbody2D body;

        private int patrolDirection = 1;
        private float patrolCenterX;
        private float nextDirectionFlipTime;
        private bool movementFrozen;
        private Vector3 shakeStartPosition;
        private Tween shakeTween;

        private void Awake()
        {
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            if (body == null) body = GetComponent<Rigidbody2D>();

            if (monsterHealth != null)
            {
                monsterHealth.HealthChanged += OnHealthChanged;
                monsterHealth.Died += OnBossDied;
            }
            else
            {
                Debug.LogWarning("FlyEvilMistyBoss: no MonsterHealth assigned — the boss cannot take damage or die.", this);
            }
        }

        private void Start()
        {
            patrolCenterX = patrolAnchor != null ? patrolAnchor.position.x : transform.position.x;
            PlanNextDirectionFlip();
            StartCoroutine(RunAttackLoop());
        }

        private void OnDestroy()
        {
            if (monsterHealth != null)
            {
                monsterHealth.HealthChanged -= OnHealthChanged;
                monsterHealth.Died -= OnBossDied;
            }

            KillShakeTween();
            StopAllCoroutines();
        }

        private void FixedUpdate()
        {
            if (movementFrozen) return;

            Patrol();
        }

        /// <summary>Stops the attack loop and patrol (called when the boss dies).</summary>
        public void StopAttacks()
        {
            KillShakeTween();
            StopAllCoroutines();
        }

        private void Patrol()
        {
            // The boss stays near the bottom-center of the arena: it may only
            // stray horizontalRange units from its anchor on each side.
            float minX = patrolCenterX - horizontalRange;
            float maxX = patrolCenterX + horizontalRange;
            float x = transform.position.x;

            if (x <= minX) patrolDirection = 1;
            else if (x >= maxX) patrolDirection = -1;

            if (Time.time >= nextDirectionFlipTime)
            {
                patrolDirection = -patrolDirection;
                PlanNextDirectionFlip();
            }

            Vector2 step = new Vector2(patrolDirection * horizontalSpeed, 0f) * Time.fixedDeltaTime;

            if (body != null)
                body.MovePosition(body.position + step);
            else
                transform.position += (Vector3)step;
        }

        private void PlanNextDirectionFlip()
        {
            float interval = Random.Range(
                Mathf.Max(0.05f, directionChangeInterval - directionChangeIntervalVariation),
                Mathf.Max(0.1f, directionChangeInterval + directionChangeIntervalVariation));

            nextDirectionFlipTime = Time.time + interval;
        }

        private bool BossAlive()
        {
            return monsterHealth == null || monsterHealth.IsAlive;
        }

        private bool IsPlayerAvailable()
        {
            if (attackTarget == null) return false;
            if (onlyAttackWhilePlayerActive && !attackTarget.gameObject.activeInHierarchy) return false;
            if (attackTargetHealth != null && !attackTargetHealth.IsAlive) return false;

            return true;
        }

        private IEnumerator RunAttackLoop()
        {
            yield return new WaitForSeconds(firstAttackDelay);

            while (BossAlive())
            {
                if (IsPlayerAvailable())
                    yield return TriangleVolley();

                yield return new WaitForSeconds(attackInterval);
            }
        }

        private IEnumerator TriangleVolley()
        {
            if (trianglePrefab == null)
            {
                Debug.LogWarning("FlyEvilMistyBoss: no trianglePrefab assigned — volley skipped.", this);
                yield break;
            }

            // Telegraph: shake in place so the player knows a volley is coming.
            movementFrozen = true;
            yield return Telegraph();

            // The whole fan is locked to the player's position at volley start.
            Vector2 aimDirection = GetAimDirection();
            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            movementFrozen = false;

            // One triangle on the aim line, then one below it and one above it.
            float[] angleOffsets = { 0f, -spreadAngle, spreadAngle };

            for (int i = 0; i < angleOffsets.Length; i++)
            {
                if (!BossAlive() || !IsPlayerAvailable())
                    yield break;

                float angle = (aimAngle + angleOffsets[i]) * Mathf.Deg2Rad;
                SpawnTriangle(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));

                if (i < angleOffsets.Length - 1 && triangleSpawnInterval > 0f)
                    yield return new WaitForSeconds(triangleSpawnInterval);
            }
        }

        private IEnumerator Telegraph()
        {
            if (telegraphDuration <= 0f) yield break;

            if (telegraphShakeStrength <= 0f || telegraphShakeVibrato <= 0)
            {
                yield return new WaitForSeconds(telegraphDuration);
                yield break;
            }

            // Patrol is frozen during the shake, so the position restored on
            // completion is the same one the shake started from.
            shakeStartPosition = transform.position;
            if (shakeTween.IsActive()) shakeTween.Complete();
            shakeTween = transform.DOShakePosition(telegraphDuration, telegraphShakeStrength, telegraphShakeVibrato)
                .OnUpdate(() => transform.position = new Vector3(transform.position.x, transform.position.y, 0f))
                .OnComplete(() => transform.position = shakeStartPosition);
            yield return shakeTween.WaitForCompletion();
            shakeTween = null;
        }

        private void KillShakeTween()
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();

                // A killed shake never runs OnComplete, so put the boss back where it started.
                if (shakeStartPosition != Vector3.zero && transform.position != shakeStartPosition)
                    transform.position = shakeStartPosition;
            }

            shakeTween = null;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            // A hit during the telegraph must not leave the boss shaking or offset.
            KillShakeTween();
        }

        private void OnBossDied()
        {
            StopAttacks();
        }

        private Vector2 GetAimDirection()
        {
            if (attackTarget == null) return Vector2.up;

            Vector2 direction = (Vector2)attackTarget.position - (Vector2)GetSpawnPosition();

            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        }

        private Vector3 GetSpawnPosition()
        {
            return transform.position + (Vector3)spawnOffset;
        }

        private void SpawnTriangle(Vector2 direction)
        {
            GameObject missileGO = Instantiate(trianglePrefab, GetSpawnPosition(), Quaternion.identity);

            BossTriangleMissile missile = missileGO.GetComponent<BossTriangleMissile>();
            if (missile != null)
                missile.Configure(triangleSpeed, triangleDamage, triangleDamageRadius, triangleLifetime, direction);
        }

        private void OnDrawGizmosSelected()
        {
            float centerX = patrolAnchor != null ? patrolAnchor.position.x : transform.position.x;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(centerX - horizontalRange, transform.position.y - 1f, 0f), new Vector3(centerX - horizontalRange, transform.position.y + 1f, 0f));
            Gizmos.DrawLine(new Vector3(centerX + horizontalRange, transform.position.y - 1f, 0f), new Vector3(centerX + horizontalRange, transform.position.y + 1f, 0f));
            Gizmos.DrawLine(new Vector3(centerX, transform.position.y, 0f), new Vector3(centerX, transform.position.y + 2.5f, 0f));
        }
    }
}
