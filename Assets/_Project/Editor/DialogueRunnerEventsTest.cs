using System;
using System.Collections.Generic;
using ChainSawLeg.Composition;
using ChainSawLeg.Features.Dialogue;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Tests
{
    public static class DialogueRunnerEventsTest
    {
        [MenuItem("ChainSawLeg/Dialogue/Test Runner Events")]
        public static void Run()
        {
            Debug.Log("========================================");
            Debug.Log("DIALOGUE RUNNER EVENTS TEST");
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
            // Arrange: создаём контейнер и загружаем реальные диалоги.
            var container = new DiContainer();

            DialogueInstaller.Install(container);

            var databaseService =
                container.Resolve<DialogueDatabaseService>();

            databaseService.Initialize().GetAwaiter().GetResult();

            IDialogueRunner runner =
                container.Resolve<IDialogueRunner>();

            Debug.Log("[PASS] Runner and database initialized");

            // Arrange: подписываемся на события.
            var eventLog = new List<string>();

            Dialogue startedDialogue = null;
            Dialogue endedDialogue = null;

            var changedNodes = new List<string>();

            runner.DialogueStarted += dialogue =>
            {
                startedDialogue = dialogue;
                eventLog.Add($"Started:{dialogue.Id}");

                Debug.Log($"[EVENT] DialogueStarted: {dialogue.Id}");
            };

            runner.NodeChanged += node =>
            {
                changedNodes.Add(node.Id);
                eventLog.Add($"NodeChanged:{node.Id}");

                Debug.Log($"[EVENT] NodeChanged: {node.Id}");
            };

            runner.DialogueEnded += dialogue =>
            {
                endedDialogue = dialogue;
                eventLog.Add($"Ended:{dialogue.Id}");

                Debug.Log($"[EVENT] DialogueEnded: {dialogue.Id}");
            };

            // Act: проходим первую ветку guard_intro.
            runner.Start("guard_intro");

            runner.Continue(); // node_001 -> node_002
            runner.Continue(); // node_002 -> node_003

            runner.SelectChoice("guard_intro_choice_001");
            // node_003 -> node_004

            runner.Continue(); // node_004 -> node_005
            runner.Continue(); // завершение диалога

            // Assert: проверяем DialogueStarted.
            Assert(
                startedDialogue != null,
                "DialogueStarted was not invoked.");

            Assert(
                startedDialogue.Id == "guard_intro",
                $"Unexpected started dialogue: {startedDialogue.Id}");

            Debug.Log("[PASS] DialogueStarted");

            // Assert: проверяем NodeChanged.
            string[] expectedNodes =
            {
                "node_001",
                "node_002",
                "node_003",
                "node_004",
                "node_005"
            };

            Assert(
                changedNodes.Count == expectedNodes.Length,
                $"Expected {expectedNodes.Length} NodeChanged events, " +
                $"but received {changedNodes.Count}.");

            for (int i = 0; i < expectedNodes.Length; i++)
            {
                Assert(
                    changedNodes[i] == expectedNodes[i],
                    $"NodeChanged event #{i + 1}: expected " +
                    $"'{expectedNodes[i]}', got '{changedNodes[i]}'.");
            }

            Debug.Log("[PASS] NodeChanged: all 5 nodes received in order");

            // Assert: проверяем DialogueEnded.
            Assert(
                endedDialogue != null,
                "DialogueEnded was not invoked.");

            Assert(
                endedDialogue.Id == "guard_intro",
                $"Unexpected ended dialogue: {endedDialogue.Id}");

            Assert(
                runner.State == DialogueRuntimeState.Ended,
                $"Expected Ended state, got '{runner.State}'.");

            Debug.Log("[PASS] DialogueEnded");

            // Assert: проверяем полный порядок событий.
            string[] expectedEvents =
            {
                "Started:guard_intro",
                "NodeChanged:node_001",
                "NodeChanged:node_002",
                "NodeChanged:node_003",
                "NodeChanged:node_004",
                "NodeChanged:node_005",
                "Ended:guard_intro"
            };

            Assert(
                eventLog.Count == expectedEvents.Length,
                $"Expected {expectedEvents.Length} total events, " +
                $"but received {eventLog.Count}.");

            for (int i = 0; i < expectedEvents.Length; i++)
            {
                Assert(
                    eventLog[i] == expectedEvents[i],
                    $"Event #{i + 1}: expected '{expectedEvents[i]}', " +
                    $"got '{eventLog[i]}'.");
            }

            Debug.Log("[PASS] Complete event order");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    $"ASSERTION FAILED: {message}");
            }
        }
    }
}