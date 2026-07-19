using NUnit.Framework;
using Zenject;

public sealed class ArchitectureRegistrationTests
{
    [Test]
    public void AutoBinderRegistersPhasesFromBaseTypeAssembly()
    {
        var container = new DiContainer();

        AutoBinder.BindDerivedTypes<GamePhase>(container);

        Assert.That(container.HasBinding<ExplorationPhase>(), Is.True);
        Assert.That(container.HasBinding<WorldOldExplorationPhase>(), Is.True);
    }
}
