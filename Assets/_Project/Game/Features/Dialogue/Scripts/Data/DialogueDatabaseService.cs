using System;
using System.Collections.Generic;
using System.IO;
using ChainSawLeg.Game.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueDatabaseService
        : ApplicationServiceBase, IDialogueDatabase
    {
        private const string RelativeJsonPath =
            "_Project/Game/Features/Dialogue/Scripts/Data/Replicas/University.json";

        private readonly DialogueImporter importer;

        private Dictionary<string, Dialogue> dialoguesById =
            new Dictionary<string, Dialogue>(StringComparer.Ordinal);

        public bool IsBuilt { get; private set; }

        public DialogueDatabaseService(DialogueImporter importer)
        {
            this.importer = importer;
        }

        public override UniTask Initialize()
        {
            string jsonPath = Path.Combine(
                Application.dataPath,
                RelativeJsonPath);

            if (!File.Exists(jsonPath))
            {
                IsBuilt = false;

                Debug.LogError(
                    $"[DialogueDatabase] JSON file not found: {jsonPath}");

                return UniTask.CompletedTask;
            }

            List<Dialogue> importedDialogues = importer.Import(jsonPath);

            if (importedDialogues == null)
            {
                IsBuilt = false;

                Debug.LogError(
                    "[DialogueDatabase] Failed to import dialogues.");

                return UniTask.CompletedTask;
            }

            Dictionary<string, Dialogue> newDatabase =
                new Dictionary<string, Dialogue>(
                    StringComparer.Ordinal);

            foreach (Dialogue dialogue in importedDialogues)
            {
                if (dialogue == null)
                {
                    IsBuilt = false;

                    Debug.LogError(
                        "[DialogueDatabase] Imported dialogue is null.");

                    return UniTask.CompletedTask;
                }

                if (string.IsNullOrWhiteSpace(dialogue.Id))
                {
                    IsBuilt = false;

                    Debug.LogError(
                        "[DialogueDatabase] Dialogue has an empty ID.");

                    return UniTask.CompletedTask;
                }

                if (newDatabase.ContainsKey(dialogue.Id))
                {
                    IsBuilt = false;

                    Debug.LogError(
                        $"[DialogueDatabase] Duplicate dialogue ID: {dialogue.Id}");

                    return UniTask.CompletedTask;
                }

                newDatabase.Add(dialogue.Id, dialogue);
            }

            dialoguesById = newDatabase;
            IsBuilt = true;

            Debug.Log(
                $"[DialogueDatabase] Successfully built. " +
                $"Loaded dialogues: {dialoguesById.Count}");

            return UniTask.CompletedTask;
        }

        public Dialogue GetDialogue(string dialogueId)
        {
            if (TryGetDialogue(dialogueId, out Dialogue dialogue))
            {
                return dialogue;
            }

            throw new KeyNotFoundException(
                $"[DialogueDatabase] Dialogue not found: '{dialogueId}'.");
        }

        public bool TryGetDialogue(
            string dialogueId,
            out Dialogue dialogue)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                dialogue = null;
                return false;
            }

            return dialoguesById.TryGetValue(dialogueId, out dialogue);
        }

        public bool Contains(string dialogueId)
        {
            return !string.IsNullOrWhiteSpace(dialogueId)
                && dialoguesById.ContainsKey(dialogueId);
        }

        public IReadOnlyCollection<Dialogue> GetAll()
        {
            return dialoguesById.Values;
        }

        public override UniTask Dispose()
        {
            dialoguesById.Clear();
            IsBuilt = false;

            return UniTask.CompletedTask;
        }
    }
}