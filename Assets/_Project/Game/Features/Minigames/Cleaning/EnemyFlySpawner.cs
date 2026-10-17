using System.Collections;
using ChainSawLeg.Features.Exploration;
using ChainSawLeg.Game.Shared;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class EnemyFlySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyFlyPrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float spawnRate = 3f;
        [SerializeField] private float delayBeforeSpawn = 3f;

        private IGameObjectSpawner gameObjectSpawner;
        private CameraFlow cameraFlow;
        private Rigidbody2D playerRigidbody2D;
        
        private Coroutine spawnCoroutine;
        private bool isInitialized;

        public void Initialize(
            IGameObjectSpawner gameObjectSpawner,
            Rigidbody2D playerRigidbody2D,
            CameraFlow cameraFlow)
        {
            this.gameObjectSpawner = gameObjectSpawner;
            this.playerRigidbody2D = playerRigidbody2D;
            this.cameraFlow = cameraFlow;

            isInitialized = true;
        }

        public void StartSpawning()
        {
            if (!isInitialized)
            {
                Debug.LogError(
                    $"{nameof(EnemyFlySpawner)} has not been initialized.",
                    this);

                return;
            }

            if (spawnCoroutine != null)
                return;

            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }

        private IEnumerator SpawnEnemies()
        {
            yield return new WaitForSeconds(delayBeforeSpawn);

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPosition = transform.position
                                        + Vector3.up * Random.Range(-2f, 2f);

                GameObject enemyObject = gameObjectSpawner.Spawn(
                    enemyFlyPrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity);

                FlyEnemy enemy = enemyObject.GetComponent<FlyEnemy>();
                
                enemy.Initialize(playerRigidbody2D, cameraFlow);

                if (enemy == null)
                {
                    Debug.LogError(
                        "Spawned prefab does not contain FlyEnemy.",
                        enemyObject);

                    yield break;
                }

                // Здесь потребуется инициализация FlyEnemy,
                // если ему необходимы дополнительные зависимости.

                yield return new WaitForSeconds(spawnRate);
            }
        }
    }
}
