using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Makes a boss climb the level as it gets hurt: every time it takes damage it
    /// is placed on the next room above the one it currently occupies.
    /// <see cref="RoomCameraPoint"/> triggers define the rooms; when no rooms are
    /// assigned they are collected from the scene and sorted bottom-to-top.
    /// </summary>
    public sealed class BossRoomProgression : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;

        [Header("Rooms")]
        [Tooltip("Room points to climb through. When empty, every RoomCameraPoint in the scene is used.")]
        [SerializeField] private List<RoomCameraPoint> rooms = new List<RoomCameraPoint>();
        [Tooltip("When the boss is already in the top room, should the next hit send it back to the bottom room?")]
        [SerializeField] private bool wrapToFirstRoom;

        [Header("Placement")]
        [Tooltip("Specific boss position per room, in room order (bottom room = index 0). Leave an entry empty to fall back to that room's RoomCameraPoint plus the offset.")]
        [SerializeField] private Transform[] roomPositions = new Transform[0];
        [Tooltip("Fallback offset from the room point's position, in world units, used only when no Transform is assigned for that room.")]
        [SerializeField] private Vector2 placementOffset;
        [SerializeField, Min(0f)] private float moveDuration = 0.25f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private bool useUnscaledTime;

        [Header("Physics")]
        [SerializeField] private bool resetVelocityAfterMove = true;

        private Rigidbody2D body;
        private int currentRoomIndex = -1;
        private Tween moveTween;

        /// <summary>True while the boss is being tweened into another room.</summary>
        public bool IsMovingBetweenRooms => moveTween != null && moveTween.IsActive();

        private void Awake()
        {
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            body = GetComponent<Rigidbody2D>();

            CollectRooms();
            currentRoomIndex = FindRoomIndexAt(transform.position);

            if (monsterHealth != null)
            {
                monsterHealth.HealthChanged += OnHealthChanged;
                monsterHealth.Died += OnBossDied;
            }
        }

        private void OnDestroy()
        {
            if (monsterHealth != null)
            {
                monsterHealth.HealthChanged -= OnHealthChanged;
                monsterHealth.Died -= OnBossDied;
            }

            KillMoveTween();
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            // The killing hit must not move the boss: it dies where it took the hit.
            if (monsterHealth != null && !monsterHealth.IsAlive) return;

            MoveToNextRoom();
        }

        private void OnBossDied()
        {
            KillMoveTween();
        }

        private void MoveToNextRoom()
        {
            if (rooms == null || rooms.Count == 0) return;

            int nextIndex;

            if (currentRoomIndex < 0)
            {
                // Boss was not inside a room yet: start from the room it is in/near.
                int startIndex = FindRoomIndexAt(transform.position);
                nextIndex = startIndex < 0 ? 0 : startIndex + 1;
            }
            else
            {
                nextIndex = currentRoomIndex + 1;
            }

            if (nextIndex >= rooms.Count)
            {
                if (!wrapToFirstRoom) return;

                nextIndex = 0;
            }

            rooms[currentRoomIndex].ActivatePlatformsToNextRoom();

            currentRoomIndex = nextIndex;
            MoveTo(GetPlacementPosition(nextIndex, rooms[nextIndex]));
        }

        private void CollectRooms()
        {
            if (rooms == null) rooms = new List<RoomCameraPoint>();

            if (rooms.Count == 0)
                rooms.AddRange(FindObjectsByType<RoomCameraPoint>(FindObjectsSortMode.None));

            rooms.RemoveAll(room => room == null);
            rooms.Sort((a, b) => GetRoomCenter(a).y.CompareTo(GetRoomCenter(b).y));
        }

        private int FindRoomIndexAt(Vector3 position)
        {
            if (rooms == null || rooms.Count == 0) return -1;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (TryGetRoomBounds(rooms[i], out Bounds bounds) && bounds.Contains(position))
                    return i;
            }

            // Not inside any room (between rooms or outside them): pick the nearest room.
            int nearest = -1;
            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < rooms.Count; i++)
            {
                Vector3 center = GetRoomCenter(rooms[i]);
                float distance = Mathf.Abs(center.y - position.y);

                if (distance >= nearestDistance) continue;

                nearestDistance = distance;
                nearest = i;
            }

            return nearest;
        }

        private Vector3 GetPlacementPosition(int roomIndex, RoomCameraPoint room)
        {
            Vector3 position;

            if (roomPositions != null
                && roomIndex >= 0
                && roomIndex < roomPositions.Length
                && roomPositions[roomIndex] != null)
            {
                position = roomPositions[roomIndex].position;
            }
            else
            {
                position = room.transform.position + (Vector3)placementOffset;
            }

            position.z = transform.position.z;

            return position;
        }

        private void MoveTo(Vector3 target)
        {
            KillMoveTween();

            if (moveDuration <= 0f)
            {
                ApplyPosition(target);
                return;
            }

            // The tween drives the transform, so physics must not fight it mid-flight.
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            moveTween = transform.DOMove(target, moveDuration)
                .SetEase(moveEase)
                .SetUpdate(useUnscaledTime)
                .OnUpdate(() => transform.position = new Vector3(transform.position.x, transform.position.y, 0f))
                .OnComplete(() => ApplyPosition(target));
        }

        private void ApplyPosition(Vector3 target)
        {
            transform.position = target;

            if (body == null) return;

            body.position = target;

            // Do not wake physics back up if the boss died while it was moving.
            if (monsterHealth != null && !monsterHealth.IsAlive) return;

            body.simulated = true;

            if (resetVelocityAfterMove) body.linearVelocity = Vector2.zero;
        }

        private void KillMoveTween()
        {
            if (moveTween != null && moveTween.IsActive()) moveTween.Kill();

            moveTween = null;
        }

        private static Vector3 GetRoomCenter(RoomCameraPoint room)
        {
            return TryGetRoomBounds(room, out Bounds bounds)
                ? bounds.center
                : room.transform.position;
        }

        private static bool TryGetRoomBounds(RoomCameraPoint room, out Bounds bounds)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();

            if (roomCollider != null)
            {
                bounds = roomCollider.bounds;
                return true;
            }

            bounds = new Bounds(room.transform.position, Vector3.zero);
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (rooms == null) return;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i] == null) continue;

                bool isCurrent = i == currentRoomIndex;

                Gizmos.color = isCurrent ? Color.cyan : Color.yellow;
                Gizmos.DrawWireSphere(rooms[i].transform.position, 0.75f);

                if (TryGetRoomBounds(rooms[i], out Bounds bounds))
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
            }

            if (roomPositions == null) return;

            for (int i = 0; i < roomPositions.Length; i++)
            {
                if (roomPositions[i] == null) continue;

                Gizmos.color = i == currentRoomIndex ? Color.green : new Color(0.4f, 1f, 0.4f, 0.6f);
                Gizmos.DrawWireSphere(roomPositions[i].position, 0.5f);
            }
        }
    }
}
