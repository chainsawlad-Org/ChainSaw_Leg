// Placement: Docs/Ru/02_ProjectStructure.md:98-116. Quote: "StateMachine знает только о жизненном цикле фаз."

using Cysharp.Threading.Tasks;

public interface IGamePhase
{
    UniTask Enter();
    UniTask Exit();
}
