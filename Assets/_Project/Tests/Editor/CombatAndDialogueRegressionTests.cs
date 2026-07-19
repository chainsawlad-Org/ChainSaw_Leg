using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class CombatAndDialogueRegressionTests
{
    [Test]
    public void TurnSystemReportsBattleOverWhenEnemyDies()
    {
        var player = new Unit("Player", 100);
        var enemy = new Unit("Enemy", 10);
        var turnSystem = new TurnSystem(
            new List<Unit> { player },
            new List<Unit> { enemy });

        enemy.TakeDamage(10);

        Assert.That(turnSystem.IsBattleOver(), Is.True);
    }

    [Test]
    public void BattleManagerEndsBattleAfterTargetDies()
    {
        var player = new Unit("Player", 100);
        var enemy = new Unit("Enemy", 10);
        var controller = new PlayerActionController();
        var manager = new BattleManager(
            new TurnSystem(
                new List<Unit> { player },
                new List<Unit> { enemy }),
            new CombatResolver(new CombatEventBus()),
            new SimpleAI(),
            controller);

        controller.SelectAction(ActionType.Attack);
        manager.Update();

        Assert.That(manager.IsBattleOver, Is.True);
        Assert.That(manager.IsPlayerVictory, Is.True);
    }

    [Test]
    public void BattleSessionRestoresOriginAndMarksDefeatedEncounterAfterVictory()
    {
        var session = new BattleSessionService();
        session.BeginEncounter(
            "world_enemy_2",
            3.5f,
            -7.25f,
            SceneNames.WorldOld);

        session.CompleteCurrentEncounter(playerWon: true);

        Assert.That(session.IsEncounterDefeated("world_enemy_2"), Is.True);
        Assert.That(session.TryConsumeReturnPosition(out float positionX, out float positionY), Is.True);
        Assert.That(positionX, Is.EqualTo(3.5f));
        Assert.That(positionY, Is.EqualTo(-7.25f));
        Assert.That(session.TryConsumeReturnPosition(out _, out _), Is.False);
        Assert.That(session.TryConsumeReturnSceneName(out string sceneName), Is.True);
        Assert.That(sceneName, Is.EqualTo(SceneNames.WorldOld));
        Assert.That(session.TryConsumeReturnSceneName(out _), Is.False);
    }

    [Test]
    public void CombatEventBusDoesNotShareSubscribersBetweenInstances()
    {
        var firstBus = new CombatEventBus();
        var secondBus = new CombatEventBus();
        var unit = new Unit("Player", 100);
        int invocationCount = 0;

        firstBus.ActionPerformed += (_, _) => invocationCount++;

        secondBus.PublishActionPerformed(unit, ActionType.Attack);

        Assert.That(invocationCount, Is.Zero);
    }

    [Test]
    public void DialogueRegistryClearsOnlyRegisteredManager()
    {
        var registry = new DialogueRuntimeRegistry();
        var registeredObject = new GameObject("RegisteredDialogueManager");
        var otherObject = new GameObject("OtherDialogueManager");
        DialogueManager registered = registeredObject.AddComponent<DialogueManager>();
        DialogueManager other = otherObject.AddComponent<DialogueManager>();

        try
        {
            registry.Register(registered);
            registry.Unregister(other);
            Assert.That(registry.Current, Is.SameAs(registered));

            registry.Unregister(registered);
            Assert.That(registry.Current, Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(registeredObject);
            Object.DestroyImmediate(otherObject);
        }
    }
}
