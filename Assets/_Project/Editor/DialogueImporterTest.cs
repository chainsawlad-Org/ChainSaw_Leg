using System.IO;
using System.Text;
using ChainSawLeg.Features.Dialogue;
using UnityEditor;
using UnityEngine;

public static class DialogueImporterTest
{
    private const string JsonRelativePath =
        "_Project/Game/Features/Dialogue/Scripts/Data/Replicas/University.json";

    [MenuItem("ChainSawLeg/Dialogue/Test Importer")]
    public static void TestImporter()
    {
        string jsonPath = Path.Combine(
            Application.dataPath,
            JsonRelativePath);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError(
                "[DialogueImporterTest] JSON file not found.\n" +
                $"Expected path: {jsonPath}");

            return;
        }

        DialogueImporter importer = new DialogueImporter(
            new DialogueJsonReader(),
            new DialogueValidator(),
            new DialogueBuilder());

        var dialogues = importer.Import(jsonPath);

        if (dialogues == null)
        {
            Debug.LogError(
                "[DialogueImporterTest] Import failed. " +
                "Check the errors above.");

            return;
        }

        StringBuilder report = new StringBuilder();

        report.AppendLine("==================================");
        report.AppendLine("DIALOGUE IMPORT TEST: SUCCESS");
        report.AppendLine("==================================");
        report.AppendLine($"File: {jsonPath}");
        report.AppendLine($"Dialogues: {dialogues.Count}");
        report.AppendLine();

        int totalNodes = 0;
        int totalChoices = 0;

        foreach (Dialogue dialogue in dialogues)
        {
            report.AppendLine($"Dialogue: {dialogue.Id}");
            report.AppendLine($"Title: {dialogue.Title}");
            report.AppendLine($"Start node: {dialogue.StartNodeId}");
            report.AppendLine($"Nodes: {dialogue.Nodes.Count}");

            totalNodes += dialogue.Nodes.Count;

            foreach (DialogueNode node in dialogue.Nodes.Values)
            {
                report.AppendLine($"  [{node.Id}] {node.Type}");
                report.AppendLine($"      Speaker: {node.Speaker}");
                report.AppendLine($"      Text: {node.Text}");

                if (!string.IsNullOrWhiteSpace(node.NextNodeId))
                {
                    report.AppendLine(
                        $"      Next node: {node.NextNodeId}");
                }

                foreach (DialogueChoice choice in node.Choices)
                {
                    report.AppendLine(
                        $"      Choice: [{choice.Id}] {choice.Text}");

                    report.AppendLine(
                        $"          -> {choice.NextNodeId}");

                    totalChoices++;
                }
            }

            report.AppendLine();
        }

        report.AppendLine("----------------------------------");
        report.AppendLine($"Total dialogues: {dialogues.Count}");
        report.AppendLine($"Total nodes: {totalNodes}");
        report.AppendLine($"Total choices: {totalChoices}");
        report.AppendLine("==================================");

        Debug.Log(report.ToString());
    }
}