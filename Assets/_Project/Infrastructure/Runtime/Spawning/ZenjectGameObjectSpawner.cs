using ChainSawLeg.Game.Shared;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Infrastructure
{
    public class ZenjectGameObjectSpawner : IGameObjectSpawner
    {
        private readonly DiContainer container;
        
        public ZenjectGameObjectSpawner(DiContainer container)
        {
            this.container = container;
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return container.InstantiatePrefab(prefab, position, rotation, parent);
        }
    }
}