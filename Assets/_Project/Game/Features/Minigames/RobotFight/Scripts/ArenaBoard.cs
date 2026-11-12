using System;
using System.Collections.Generic;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// View facade of the fight arena: builds and owns the tile visuals, delegates grid
    /// logic to <see cref="ArenaGridModel"/>, tracks player health, forwards it to the
    /// <see cref="ArenaHudView"/>, and hands the camera over to / back from the arena
    /// via <see cref="ArenaCameraRig"/>.
    /// </summary>
    public class ArenaBoard : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private RobotFightController controller;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private Transform cameraPoint;
        [SerializeField] private Transform arenaCenterPoint;
        [SerializeField] private RectTransform healthbarTransform;
        [SerializeField] private Image healthbarImage;
        [SerializeField] private CanvasGroup loseScreen;
        [SerializeField] private ArenaCell cellPrefab;

        [Header("Board")]
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private int playerHealth = 100;

        [Header("Camera")]
        [SerializeField] private float cameraPointSpeed = 10f;
        [Tooltip("Seconds the camera pan to the arena centre takes.")]
        [SerializeField] private float cameraMoveDuration = 1f;

        [Header("HUD Animation (defaults reproduce the original behaviour)")]
        [Tooltip("Screen-space X position the healthbar slides to when shown; hidden is the negated value.")]
        [SerializeField] private float healthbarShownX = 100f;
        [Tooltip("Seconds the healthbar slide in/out takes.")]
        [SerializeField] private float healthbarSlideDuration = 1f;
        [Tooltip("Seconds the healthbar fill tween takes after taking damage.")]
        [SerializeField] private float healthbarFillDuration = 0.25f;
        [Tooltip("Seconds the lose-screen fade-in takes.")]
        [SerializeField] private float loseScreenFadeDuration = 1.5f;
        [Tooltip("DOTween duration parameter of the healthbar shake on player death.")]
        [SerializeField] private float loseShakeDuration = 10f;
        [Tooltip("DOTween strength parameter of the healthbar shake on player death.")]
        [SerializeField] private float loseShakeStrength = 2f;
        [Tooltip("DOTween vibrato parameter of the healthbar shake on player death.")]
        [SerializeField] private int loseShakeVibrato = 100;
        [Tooltip("DOTween randomness parameter of the healthbar shake on player death.")]
        [SerializeField] private float loseShakeRandomness = 90f;

        [Header("Board Rebuild")]
        [Tooltip("Seconds between despawning the old board and building the new one.")]
        [SerializeField] private float boardRebuildDelay = 1f;
        [Tooltip("Seconds between the new board spawning and the rebuild completing.")]
        [SerializeField] private float boardRebuildCompleteDelay = 1f;

        [Header("Feedback")]
        [Tooltip("Camera shake strength when an item lands near, but not on, the player.")]
        [SerializeField] private float nearbyHitShakeStrength = 0.2f;
        [Tooltip("Camera shake vibrato when an item lands near, but not on, the player.")]
        [SerializeField] private int nearbyHitShakeVibrato = 100;
        [Tooltip("Camera shake duration when an item lands near, but not on, the player.")]
        [SerializeField] private float nearbyHitShakeDuration = 0.4f;
        [Tooltip("Seconds after the player's kick during which a landed beatable block counts as kicked back.")]
        [SerializeField] private float beatWindow = 0.25f;

        private readonly Dictionary<Vector2Int, ArenaCell> cells = new Dictionary<Vector2Int, ArenaCell>();
        private ArenaGridModel grid;
        private PlayerHealthModel health;
        private ArenaHudView hud;
        private ArenaCameraRig cameraRig;
        private ICameraEffects cameraEffects;
        private float lastKickTime;

        /// <summary>Raised when the player kicks the beatable cell of a rebuilt arena.</summary>
        public event Action onLastCellKicked;

        /// <summary>Raised when the player's health reaches zero.</summary>
        public event Action onPlayerDead;

        /// <summary>Logical cell the arena player currently occupies.</summary>
        public Vector2Int currentPlayerCell => grid.PlayerCell;

        /// <summary>
        /// Logical cell holding the beatable block in rebuilt arenas (second row of the
        /// last column) — the kick target that ends the second phase and where the robot lands.
        /// </summary>
        public Vector2Int beatableCell => grid.BeatableCell;

        /// <summary>Whether the second fight phase (heaven spawns, growing arenas) is active.</summary>
        public bool isSecondPhase { get; private set; }

        private void Start()
        {
            grid = new ArenaGridModel(boardSize, cellSize, Vector2Int.one);
            health = new PlayerHealthModel(playerHealth);
            hud = new ArenaHudView(healthbarTransform, healthbarImage, loseScreen,
                healthbarShownX, healthbarSlideDuration, healthbarFillDuration,
                loseScreenFadeDuration, loseShakeDuration, loseShakeStrength,
                loseShakeVibrato, loseShakeRandomness);
            cameraRig = new ArenaCameraRig(cameraFlow, cameraPoint, arenaCenterPoint,
                cameraPointSpeed, cameraMoveDuration);
            cameraEffects = new CameraFlowEffects(cameraFlow);

            health.HealthChanged += hud.SetHealthFraction;
            controller.KickPerformed += RecordKick;

            BuildBoard(boardSize, animateSpawn: false, parent: null);
        }

        private void OnDestroy()
        {
            controller.KickPerformed -= RecordKick;
        }

        private void Update()
        {
            cameraRig.TrackPlayer(controller.transform.position, Time.deltaTime);
        }

        /// <summary>Spawns the arena visuals, activates the arena player and shows the healthbar.</summary>
        public void EnablePlayer()
        {
            foreach (ArenaCell cell in cells.Values)
            {
                cell.DoSpawnAnimation();
            }

            controller.gameObject.SetActive(true);
            hud.ShowHealthbar();
        }

        /// <summary>Detaches the camera from the world player and pans it to the arena centre.</summary>
        public void SetupCamera() => cameraRig.EnterArena();

        /// <summary>Restores the world camera, removes all cells and hides the healthbar.</summary>
        public void CloseArena(PlayerMovement player)
        {
            cameraRig.ExitArena(player);
            foreach (ArenaCell cell in cells.Values)
            {
                Destroy(cell.gameObject);
            }

            hud.HideHealthbar();
            cells.Clear();
        }

        /// <summary>Turns the arena player to face the robot (resets the world-direction flip).</summary>
        public void FlipPlayerToRobot() => controller.FlipPlayer(false);

        /// <summary>Knocks the arena player away by <paramref name="force"/> over <paramref name="duration"/> seconds.</summary>
        public void KickPlayer(Vector2 force, float duration)
        {
            controller.Relocate((Vector2)controller.transform.position + force, duration);
            controller.ApplyDamage(0);
        }

        /// <summary>Activates the second fight phase rules (heaven spawns, beatable-cell win condition).</summary>
        public void BeginSecondPhase() => isSecondPhase = true;

        /// <summary>Enables lateral camera tracking of the arena player.</summary>
        public void EnablePlayerCameraFollow() => cameraRig.EnablePlayerFollow();

        /// <summary>
        /// Despawns the current board and, after a delay, builds a new one of
        /// <paramref name="size"/> parented under the board transform, marking the beatable cell.
        /// </summary>
        public void CreateArenaSecondPhase(Vector2Int size, Action onComplete)
        {
            grid.SetPlayerCell(Vector2Int.up);

            foreach (ArenaCell cell in cells.Values)
            {
                cell.Despawn();
            }

            cells.Clear();

            DOVirtual.DelayedCall(boardRebuildDelay, () =>
            {
                grid.Resize(size);
                BuildBoard(size, animateSpawn: true, parent: transform);
                transform.position = (Vector2)controller.transform.position - grid.PlayerCell * cellSize;
                DOVirtual.DelayedCall(boardRebuildCompleteDelay, () => onComplete?.Invoke());
                cells[grid.BeatableCell].SetBeatableCell();
            });
        }

        /// <summary>Attempts to move the arena player one cell in the dominant input direction.</summary>
        /// <param name="moveInput">Raw move input vector.</param>
        /// <param name="newPosition">World position of the target cell when the move succeeds.</param>
        public bool TryMove(Vector2 moveInput, out Vector2 newPosition)
        {
            newPosition = Vector2.zero;

            if (!grid.TryMove(moveInput, out Vector2Int nextCell)) return false;

            newPosition = cells[nextCell].transform.position;
            return true;
        }

        /// <summary>
        /// Called on every player kick; in the second phase, kicking the beatable cell
        /// raises <see cref="onLastCellKicked"/>.
        /// </summary>
        public void KickCell()
        {
            if (!isSecondPhase) return;

            if (grid.PlayerCell == grid.BeatableCell)
            {
                onLastCellKicked?.Invoke();
            }
        }

        /// <summary>Reverts a cell to its neutral state.</summary>
        public void SetCellDefault(Vector2Int cell) => cells[cell].SetDefaultCell();

        /// <summary>Marks a cell as an incoming attack's danger zone.</summary>
        public void SetCellDanger(Vector2Int cell) => cells[cell].SetDangerCell();

        /// <summary>Marks a cell as holding a block the player can kick back.</summary>
        public void SetCellBeatable(Vector2Int cell) => cells[cell].SetBeatableCell();

        /// <summary>
        /// Damages the player if they stand on <paramref name="cell"/>; otherwise plays a
        /// small camera shake for the near miss.
        /// </summary>
        public void TakeDamageToCell(Vector2Int cell, int damage)
        {
            if (grid.PlayerCell != cell)
            {
                cameraEffects.Shake(nearbyHitShakeStrength, nearbyHitShakeVibrato, nearbyHitShakeDuration);
                return;
            }

            if (health.IsDepleted) return;

            AudioService.Play(AudioId.DamageTaken);
            health.ApplyDamage(damage);
            controller.ApplyDamage(damage);

            if (health.IsDepleted)
            {
                hud.ShowLoseScreen();
                onPlayerDead?.Invoke();
            }
        }

        /// <summary>World position of a cell.</summary>
        public Vector2 GetCellPosition(Vector2Int cell) => cells[cell].transform.position;

        /// <summary>Current board dimensions in cells.</summary>
        public Vector2Int GetBoardSize() => grid.BoardSize;

        /// <summary>True when the cell exists and is in its neutral state.</summary>
        public bool CellIsAvailable(Vector2Int cell) =>
            cells.ContainsKey(cell) && cells[cell].inDefaultState;

        /// <summary>
        /// True when the player stands on <paramref name="cell"/> and performed a kick
        /// within the beat window — the kick-back check for beatable blocks.
        /// </summary>
        public bool CellIsBeated(Vector2Int cell) =>
            grid.PlayerCell == cell && Time.time - lastKickTime < beatWindow;

        /// <summary>Enables or disables input handling for the arena player.</summary>
        public void SetActivePlayer(bool active) => controller.SetControlActive(active);

        private void RecordKick() => lastKickTime = Time.time;

        private void BuildBoard(Vector2Int size, bool animateSpawn, Transform parent)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2 cellPosition = transform.position + new Vector3(x * cellSize.x, y * cellSize.y, 0);
                    ArenaCell cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, parent);
                    if (animateSpawn)
                    {
                        cell.DoSpawnAnimation();
                    }

                    cells.Add(new Vector2Int(x, y), cell);
                }
            }
        }
    }
}
