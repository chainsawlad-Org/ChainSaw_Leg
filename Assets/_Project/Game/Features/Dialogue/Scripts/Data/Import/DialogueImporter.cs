using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueImporter
    {
        private readonly DialogueJsonReader reader;
        private readonly DialogueValidator validator;
        private readonly DialogueBuilder builder;

        public DialogueImporter(
            DialogueJsonReader reader,
            DialogueValidator validator,
            DialogueBuilder builder)
        {
            this.reader = reader;
            this.validator = validator;
            this.builder = builder;
        }
        
        public List<Dialogue> Import(string jsonPath)
        {
            Debug.Log($"[DialogueImporter] Starting import: {jsonPath}");

            DialogueJsonData data = reader.Read(jsonPath);

            if (data == null)
            {
                Debug.LogError(
                    "[DialogueImporter] FAILED: Reader returned null. " +
                    "Check JSON file, path and deserialization.");

                return null;
            }

            Debug.Log(
                $"[DialogueImporter] JSON read successfully. " +
                $"Dialogues: {data.Dialogues?.Count ?? 0}, " +
                $"Nodes: {data.Nodes?.Count ?? 0}, " +
                $"Choices: {data.Choices?.Count ?? 0}");

            if (!validator.Validate(data))
            {
                Debug.LogError(
                    "[DialogueImporter] FAILED: Validation failed. " +
                    "See DialogueValidator errors above.");

                return null;
            }

            Debug.Log("[DialogueImporter] Validation successful.");

            List<Dialogue> dialogues = builder.Build(data);

            if (dialogues == null)
            {
                Debug.LogError(
                    "[DialogueImporter] FAILED: Builder returned null.");

                return null;
            }

            Debug.Log(
                $"[DialogueImporter] Import successful. " +
                $"Built dialogues: {dialogues.Count}");

            return dialogues;
        }
    }
}
