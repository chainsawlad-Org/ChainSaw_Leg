using UnityEngine;

namespace ChainSawLeg.Game.Shared
{
    public interface IGameObjectSpawner
    {
        GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}