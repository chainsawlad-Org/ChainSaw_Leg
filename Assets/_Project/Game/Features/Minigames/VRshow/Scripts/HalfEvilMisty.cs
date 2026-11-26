using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Half Evil Misty boss: every attack releases a fan of three triangles from
    /// the boss body. One triangle is aimed at the player, and the other two fly
    /// 45 degrees below and above that aim line. The three are released one after
    /// another at a fixed interval. The boss only attacks while it shares a room
    /// with the player (rooms are defined by <see cref="RoomCameraPoint"/>).
    /// </summary>
    public sealed class HalfEvilMisty : MonoBehaviour
    {
        [Header("Attack Loop")]
        [SerializeField, Min(0f)] private float firstAttackDelay = 2f;
        [SerializeField, Min(0f)] private float attackInterval = 4f;

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

        [Header("Room Gating")]
        [Tooltip("Only attack while the boss and the player are inside the same RoomCameraPoint.")]
        [SerializeField] private bool onlyAttackInSameRoom = true;

        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;
        [SerializeField] private BossRoomProgression roomProgression;

        private PlatformerPlayerHealth playerHealth;
        private RoomCameraPoint[] rooms;
        private Tween shakeTween;

        private void Awake()
        {
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            if (roomProgression == null) roomProgression = GetComponent<BossRoomProgression>();

            if (monsterHealth != null)
            {
                monsterHealth.HealthChanged += OnHealthChanged;
                monsterHealth.Died += OnBossDied;
            }
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();
            rooms = FindObjectsByType<RoomCameraPoint>(FindObjectsSortMode.None);
        }

        public void StartAttack()
        {
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

        private bool BossAlive()
        {
            return monsterHealth == null || monsterHealth.IsAlive;
        }

        private void OnBossDied()
        {
            KillShakeTween();
            StopAllCoroutines();
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            // A hit ends the telegraph early: BossRoomProgression may immediately
            // tween the boss to the next room, which would fight the shake tween.
            KillShakeTween();
        }

        private IEnumerator RunAttackLoop()
        {
            yield return new WaitForSeconds(firstAttackDelay);

            while (BossAlive())
            {
                if (playerHealth != null && playerHealth.IsAlive && IsPlayerInSameRoom() && !IsMovingBetweenRooms())
                    yield return TriangleVolley();

                yield return new WaitForSeconds(attackInterval);
            }
        }

        private IEnumerator TriangleVolley()
        {
            if (trianglePrefab == null)
            {
                Debug.LogWarning("HalfEvilMisty: no trianglePrefab assigned — volley skipped.", this);
                yield break;
            }

            if (IsMovingBetweenRooms()) yield break;

            // Telegraph: shake in place so the player knows a volley is coming.
            yield return Telegraph();
            
            // The whole fan is locked to the player's position at volley start.
            Vector2 aimDirection = GetAimDirection();
            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            // One triangle on the aim line, then one 45 degrees below it and one above it.
            float[] angleOffsets = { 0f, -spreadAngle, spreadAngle };

            for (int i = 0; i < angleOffsets.Length; i++)
            {
                if (!BossAlive() || playerHealth == null || !playerHealth.IsAlive || !IsPlayerInSameRoom() || IsMovingBetweenRooms())
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

            Vector3 startPosition = transform.position;
            if (shakeTween.IsActive()) shakeTween.Complete();
            shakeTween = transform.DOShakePosition(telegraphDuration, telegraphShakeStrength, telegraphShakeVibrato)
                .OnUpdate(() => transform.position = new Vector3(transform.position.x, transform.position.y, 0f))
                .OnComplete(() => transform.position = startPosition);
            yield return shakeTween.WaitForCompletion();
            shakeTween = null;
        }

        private void KillShakeTween()
        {
            if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();

            shakeTween = null;
        }

        private Vector2 GetAimDirection()
        {
            if (playerHealth == null) return Vector2.up;

            Vector2 direction = (Vector2)playerHealth.transform.position - (Vector2)GetSpawnPosition();

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

        private bool IsMovingBetweenRooms()
        {
            return roomProgression != null && roomProgression.IsMovingBetweenRooms;
        }

        private bool IsPlayerInSameRoom()
        {
            if (!onlyAttackInSameRoom) return true;
            if (playerHealth == null) return false;

            // No room data in this scene: nothing to gate against.
            if (rooms == null || rooms.Length == 0) return true;

            RoomCameraPoint bossRoom = FindRoomAt(gameObject.GetComponent<Collider2D>());
            RoomCameraPoint playerRoom = FindRoomAt(playerHealth.GetComponent<Collider2D>());
            
            return bossRoom != null && bossRoom == playerRoom;
        }

        private RoomCameraPoint FindRoomAt(Collider2D collider)
        {
            if (rooms == null) return null;

            foreach (RoomCameraPoint room in rooms)
            {
                if (room == null) continue;

                Collider2D roomCollider = room.GetComponent<Collider2D>();

                if (roomCollider != null && roomCollider.bounds.Intersects(collider.bounds))
                    return room;
                
            }

            return null;
        }
    }
}
