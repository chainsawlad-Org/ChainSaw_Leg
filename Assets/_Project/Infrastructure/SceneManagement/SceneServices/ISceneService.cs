
// Placement: Docs/Ru/02_ProjectStructure.md:192-202. Quote: "Содержит управление сценами."

using Cysharp.Threading.Tasks;

public interface ISceneService
{
    UniTask Initialize();
    UniTask Dispose();
}
