using System;
using System.IO;
using System.Text;
using ChainSawLeg.Composition;
using ChainSawLeg.Features.Dialogue;
using UnityEditor;
using UnityEngine;
using Zenject;

public static class DialogueInstallerIntegrationTest
{
    private const string JsonRelativePath =
        "_Project/Game/Features/Dialogue/Scripts/Data/Replicas/University.json";

    [MenuItem("ChainSawLeg/Dialogue/Test Installer Integration")]
    public static async void RunTest()
    {
        try
        {
            // 1. Проверяем наличие исходного JSON.
            string jsonPath = Path.Combine(
                Application.dataPath,
                JsonRelativePath);

            Require(
                File.Exists(jsonPath),
                $"JSON file not found: {jsonPath}");

            // 2. Создаём изолированный контейнер Zenject.
            var container = new DiContainer();

            // 3. Устанавливаем наш Installer.
            DialogueInstaller.Install(container);

            // 4. Проверяем разрешение зависимостей.
            DialogueImporter importer =
                container.Resolve<DialogueImporter>();

            DialogueDatabaseService service =
                container.Resolve<DialogueDatabaseService>();

            IDialogueDatabase database =
                container.Resolve<IDialogueDatabase>();

            IApplicationService applicationService =
                container.Resolve<IApplicationService>();

            Require(importer != null,
                "DialogueImporter could not be resolved.");

            Require(service != null,
                "DialogueDatabaseService could not be resolved.");

            Require(database != null,
                "IDialogueDatabase could not be resolved.");

            Require(applicationService != null,
                "IApplicationService could not be resolved.");

            Require(
                ReferenceEquals(service, database),
                "IDialogueDatabase is not the same instance as DialogueDatabaseService.");

            Require(
                ReferenceEquals(service, applicationService),
                "IApplicationService is not the same instance as DialogueDatabaseService.");

            // 5. Инициализируем базу данных.
            await service.Initialize();

            Require(
                service.IsBuilt,
                "Dialogue database initialization failed.");

            // 6. Проверяем известные диалоги и их стартовые узлы.
            string[] expectedDialogueIds =
            {
                "guard_intro",
                "toilet_wars_intro",
                "science_fair"
            };

            var report = new StringBuilder();

            report.AppendLine("========================================");
            report.AppendLine("DIALOGUE INSTALLER INTEGRATION TEST");
            report.AppendLine("========================================");
            report.AppendLine("Installer registration: SUCCESS");
            report.AppendLine("Dependency resolution: SUCCESS");
            report.AppendLine("Database initialization: SUCCESS");
            report.AppendLine();

            foreach (string dialogueId in expectedDialogueIds)
            {
                Require(
                    database.TryGetDialogue(
                        dialogueId,
                        out Dialogue dialogue),
                    $"Dialogue not found: {dialogueId}");

                Require(
                    dialogue.TryGetNode(
                        dialogue.StartNodeId,
                        out DialogueNode startNode),
                    $"Start node not found: {dialogueId}/{dialogue.StartNodeId}");

                report.AppendLine($"Dialogue: {dialogue.Id}");
                report.AppendLine($"  Title: {dialogue.Title}");
                report.AppendLine($"  Start node: {dialogue.StartNodeId}");
                report.AppendLine($"  Start node type: {startNode.Type}");
                report.AppendLine($"  Nodes: {dialogue.Nodes.Count}");
                report.AppendLine();
            }

            report.AppendLine(
                $"Total dialogues: {database.GetAll().Count}");

            report.AppendLine("========================================");
            report.AppendLine("RESULT: SUCCESS");
            report.AppendLine("========================================");

            Debug.Log(report.ToString());
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[DialogueInstallerIntegrationTest] FAILED\n" +
                exception);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}