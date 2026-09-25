using System.Collections;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class EnemyFlySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyFlyPrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float spawnRate = 3f;

        private DiContainer container;

        [Inject]
        private void Construct(DiContainer container)
        {
            this.container = container;
        }

        public void StartSpawning()
        {
            StartCoroutine(SpawnEnemies());
        }

        IEnumerator SpawnEnemies()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                yield return new WaitForSeconds(spawnRate);
                container.InstantiatePrefab(enemyFlyPrefab, transform.position + (Vector3.up * Random.Range(-2f, 2f)), Quaternion.identity, null);
            }
        }
    }
}
