using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IGamePhase
{
    UniTask Enter();
    UniTask Exit();
}
