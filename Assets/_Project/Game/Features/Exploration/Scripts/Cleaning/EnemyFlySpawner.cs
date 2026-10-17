using System.Collections;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class EnemyFlySpawner : MonoBehaviour
    {
        [SerializeField] private FlyEnemy enemyFlyPrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float spawnRate = 3f;
        [SerializeField] private float delayBeforeSpawn = 3f;
        
        private Rigidbody2D playerRigidbody2D;
        private CameraFlow cameraFlow;

        public void Initialize(Rigidbody2D playerRigidbody2D, CameraFlow cameraFlow)
        {
            this.playerRigidbody2D = playerRigidbody2D;
            this.cameraFlow = cameraFlow;
        }

        public void StartSpawning()
        {
            StartCoroutine(SpawnEnemies());
        }

        IEnumerator SpawnEnemies()
        {
            yield return new WaitForSeconds(delayBeforeSpawn);
            
            for (int i = 0; i < enemyCount; i++)
            {
                FlyEnemy enemy = Instantiate(enemyFlyPrefab, transform.position + (Vector3.up * Random.Range(-2f, 2f)), Quaternion.identity);
                enemy.Initialize(playerRigidbody2D, cameraFlow);
                yield return new WaitForSeconds(spawnRate);
            }
        }
    }
}
