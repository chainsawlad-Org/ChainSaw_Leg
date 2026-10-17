using System;
using ChainSawLeg.Composition;
using ChainSawLeg.Features.Dialogue;
using UnityEditor;
using UnityEngine;
using Zenject;

public static class DialogueRunnerIntegrationTest
{
    [MenuItem("ChainSawLeg/Dialogue/Test Runner Integration")]
    public static void Run()
    {
        Debug.Log("========================================");
        Debug.Log("DIALOGUE RUNNER INTEGRATION TEST");
        Debug.Log("========================================");

        try
        {
            RunTest();

            Debug.Log("========================================");
            Debug.Log("RESULT: SUCCESS");
            Debug.Log("========================================");
        }
        catch (Exception exception)
        {
            Debug.LogError("========================================");
            Debug.LogError("RESULT: FAILED");
            Debug.LogError(exception);
            Debug.LogError("========================================");
        }
    }

    private static void RunTest()
    {
        // 1. Создаём отдельный DI-контейнер.
        var container = new DiContainer();

        // 2. Регистрируем зависимости диалоговой системы.
        DialogueInstaller.Install(container);

        Debug.Log("[PASS] DialogueInstaller installation");

        // 3. Получаем сервис базы данных.
        var databaseService =
            container.Resolve<DialogueDatabaseService>();

        // 4. Загружаем реальные данные из University.json.
        databaseService.Initialize().GetAwaiter().GetResult();

        IDialogueDatabase database =
            container.Resolve<IDialogueDatabase>();

        Assert(
            database.Contains("guard_intro"),
            "Database does not contain 'guard_intro'.");

        Debug.Log("[PASS] Dialogue database initialization");

        // 5. Получаем runner через DI.
        IDialogueRunner runner =
            container.Resolve<IDialogueRunner>();

        Assert(
            runner != null,
            "Failed to resolve IDialogueRunner.");

        Debug.Log("[PASS] DialogueRunner dependency resolution");

        // 6. Проверяем первую ветку диалога.
        TestFirstBranch(runner);

        // 7. Проверяем вторую ветку диалога.
        TestSecondBranch(runner);

        // 8. Проверяем принудительную остановку.
        TestStop(runner);

        Debug.Log("[PASS] All integration tests completed");
    }

    private static void TestFirstBranch(IDialogueRunner runner)
    {
        Debug.Log("--- TEST: FIRST CHOICE BRANCH ---");

        runner.Start("guard_intro");

        AssertNode(runner, "node_001");
        AssertState(
            runner,
            DialogueRuntimeState.AwaitingInput);

        runner.Continue();

        AssertNode(runner, "node_002");
        AssertState(
            runner,
            DialogueRuntimeState.AwaitingInput);

        runner.Continue();

        AssertNode(runner, "node_003");
        AssertState(
            runner,
            DialogueRuntimeState.AwaitingChoice);

        Assert(
            runner.CurrentNode.Type == DialogueNodeType.Choice,
            "Expected node_003 to be a Choice node.");

        runner.SelectChoice("guard_intro_choice_001");

        AssertNode(runner, "node_004");
        AssertState(
            runner,
            DialogueRuntimeState.AwaitingInput);

        runner.Continue();

        AssertNode(runner, "node_005");

        runner.Continue();

        AssertState(
            runner,
            DialogueRuntimeState.Ended);

        Assert(
            runner.CurrentDialogue.Id == "guard_intro",
            "Current dialogue should remain available after ending.");

        Debug.Log("[PASS] First choice branch and natural completion");
    }

    private static void TestSecondBranch(IDialogueRunner runner)
    {
        Debug.Log("--- TEST: SECOND CHOICE BRANCH ---");

        // Запускаем тот же диалог заново.
        runner.Start("guard_intro");

        AssertNode(runner, "node_001");

        runner.Continue();
        AssertNode(runner, "node_002");

        runner.Continue();
        AssertNode(runner, "node_003");

        AssertState(
            runner,
            DialogueRuntimeState.AwaitingChoice);

        runner.SelectChoice("guard_intro_choice_002");

        AssertNode(runner, "node_005");

        AssertState(
            runner,
            DialogueRuntimeState.AwaitingInput);

        runner.Continue();

        AssertState(
            runner,
            DialogueRuntimeState.Ended);

        Debug.Log("[PASS] Second choice branch and natural completion");
    }

    private static void TestStop(IDialogueRunner runner)
    {
        Debug.Log("--- TEST: STOP ---");

        runner.Start("guard_intro");

        AssertNode(runner, "node_001");

        runner.Stop();

        AssertState(
            runner,
            DialogueRuntimeState.Idle);

        Assert(
            runner.CurrentDialogue == null,
            "CurrentDialogue should be null after Stop().");

        Assert(
            runner.CurrentNode == null,
            "CurrentNode should be null after Stop().");

        Debug.Log("[PASS] Stop resets runner state");
    }

    private static void AssertNode(
        IDialogueRunner runner,
        string expectedNodeId)
    {
        Assert(
            runner.CurrentNode != null,
            $"Expected node '{expectedNodeId}', but CurrentNode is null.");

        Assert(
            runner.CurrentNode.Id == expectedNodeId,
            $"Expected node '{expectedNodeId}', " +
            $"but got '{runner.CurrentNode.Id}'.");

        Debug.Log(
            $"[PASS] Current node: {runner.CurrentNode.Id}");
    }

    private static void AssertState(
        IDialogueRunner runner,
        DialogueRuntimeState expectedState)
    {
        Assert(
            runner.State == expectedState,
            $"Expected state '{expectedState}', " +
            $"but got '{runner.State}'.");

        Debug.Log($"[PASS] State: {runner.State}");
    }

    private static void Assert(
        bool condition,
        string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(
                $"ASSERTION FAILED: {message}");
        }
    }
}