// Placement: Docs/Ru/02_ProjectStructure.md:192-202. Quote: "Содержит управление сценами."

public interface IActiveSceneProvider
{
    string ActiveSceneName { get; }
}
