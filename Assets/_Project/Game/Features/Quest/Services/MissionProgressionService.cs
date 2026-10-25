
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChainSawLeg.Features.Quest
{
    public sealed class MissionProgressionService
    {
        private readonly Dictionary<string, QuestLineDefinition> questLines =
            new Dictionary<string, QuestLineDefinition>();

        private readonly Dictionary<string, QuestDefinition> quests =
            new Dictionary<string, QuestDefinition>();

        private readonly Dictionary<string, QuestStatus> statuses =
            new Dictionary<string, QuestStatus>();

        private bool isInitialized;

        public event Action<string, QuestStatus, QuestStatus> QuestStatusChanged;

        public bool IsInitialized => isInitialized;

        public void RegisterQuestLine(QuestLineDefinition definition)
        {
            EnsureNotInitialized();

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (questLines.ContainsKey(definition.Id))
            {
                throw new InvalidOperationException(
                    $"Quest line '{definition.Id}' is already registered.");
            }

            questLines.Add(definition.Id, definition);
        }

        public void RegisterQuest(QuestDefinition definition)
        {
            EnsureNotInitialized();

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (quests.ContainsKey(definition.Id))
            {
                throw new InvalidOperationException(
                    $"Quest '{definition.Id}' is already registered.");
            }

            quests.Add(definition.Id, definition);
            statuses.Add(definition.Id, QuestStatus.Locked);
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            ValidateDefinitions();

            isInitialized = true;
            RefreshAvailability();
        }

        public bool StartQuest(string questId)
        {
            EnsureInitialized();

            if (!statuses.TryGetValue(questId, out QuestStatus status))
                return false;

            if (status != QuestStatus.Available)
                return false;

            SetStatus(questId, QuestStatus.InProgress);
            RefreshAvailability();

            return true;
        }

        public bool CompleteQuest(string questId)
        {
            EnsureInitialized();

            if (!statuses.TryGetValue(questId, out QuestStatus status))
                return false;

            if (status != QuestStatus.InProgress)
                return false;

            SetStatus(questId, QuestStatus.Completed);

            CompleteEligibleParents(questId);
            RefreshAvailability();

            return true;
        }

        public bool FailQuest(string questId)
        {
            EnsureInitialized();

            if (!statuses.TryGetValue(questId, out QuestStatus status))
                return false;

            if (status != QuestStatus.InProgress)
                return false;

            SetStatus(questId, QuestStatus.Failed);
            RefreshAvailability();

            return true;
        }

        public QuestStatus GetStatus(string questId)
        {
            EnsureInitialized();

            if (!statuses.TryGetValue(questId, out QuestStatus status))
            {
                throw new KeyNotFoundException(
                    $"Quest '{questId}' is not registered.");
            }

            return status;
        }

        public bool TryGetStatus(
            string questId,
            out QuestStatus status)
        {
            EnsureInitialized();
            return statuses.TryGetValue(questId, out status);
        }

        public bool TryGetQuestDefinition(
            string questId,
            out QuestDefinition definition)
        {
            return quests.TryGetValue(questId, out definition);
        }

        public bool TryGetQuestLineDefinition(
            string questLineId,
            out QuestLineDefinition definition)
        {
            return questLines.TryGetValue(questLineId, out definition);
        }

        private void RefreshAvailability()
        {
            bool changed;

            do
            {
                changed = false;

                foreach (KeyValuePair<string, QuestDefinition> entry in quests)
                {
                    string questId = entry.Key;
                    QuestDefinition definition = entry.Value;

                    if (statuses[questId] != QuestStatus.Locked)
                        continue;

                    if (!CanBecomeAvailable(definition))
                        continue;

                    SetStatus(questId, QuestStatus.Available);
                    changed = true;
                }
            }
            while (changed);
        }

        private bool CanBecomeAvailable(QuestDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.ParentQuestId))
            {
                if (!statuses.TryGetValue(
                        definition.ParentQuestId,
                        out QuestStatus parentStatus))
                {
                    return false;
                }

                if (parentStatus != QuestStatus.InProgress)
                    return false;
            }

            for (int i = 0;
                 i < definition.PrerequisiteQuestIds.Count;
                 i++)
            {
                string prerequisiteId =
                    definition.PrerequisiteQuestIds[i];

                if (!statuses.TryGetValue(
                        prerequisiteId,
                        out QuestStatus prerequisiteStatus))
                {
                    return false;
                }

                if (prerequisiteStatus != QuestStatus.Completed)
                    return false;
            }

            return true;
        }

        private void CompleteEligibleParents(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestDefinition child))
                return;

            string parentId = child.ParentQuestId;

            while (!string.IsNullOrWhiteSpace(parentId))
            {
                if (!quests.TryGetValue(
                        parentId,
                        out QuestDefinition parent))
                {
                    return;
                }

                if (parent.CompletionPolicy !=
                    QuestCompletionPolicy.AllChildrenCompleted)
                {
                    return;
                }

                if (parent.ChildQuestIds.Count == 0)
                    return;

                bool allCompleted = true;

                for (int i = 0; i < parent.ChildQuestIds.Count; i++)
                {
                    string childId = parent.ChildQuestIds[i];

                    if (!statuses.TryGetValue(
                            childId,
                            out QuestStatus childStatus) ||
                        childStatus != QuestStatus.Completed)
                    {
                        allCompleted = false;
                        break;
                    }
                }

                if (!allCompleted)
                    return;

                if (statuses[parentId] != QuestStatus.InProgress)
                    return;

                SetStatus(parentId, QuestStatus.Completed);

                child = parent;
                parentId = parent.ParentQuestId;
            }
        }

        private void SetStatus(string questId, QuestStatus newStatus)
        {
            QuestStatus oldStatus = statuses[questId];

            if (oldStatus == newStatus)
                return;

            statuses[questId] = newStatus;
            QuestStatusChanged?.Invoke(questId, oldStatus, newStatus);
        }

        private void ValidateDefinitions()
        {
            foreach (KeyValuePair<string, QuestDefinition> entry in quests)
            {
                QuestDefinition quest = entry.Value;

                if (!string.IsNullOrWhiteSpace(quest.ParentQuestId))
                {
                    if (!quests.TryGetValue(
                            quest.ParentQuestId,
                            out QuestDefinition parent))
                    {
                        throw new InvalidOperationException(
                            $"Quest '{quest.Id}' references missing parent " +
                            $"'{quest.ParentQuestId}'.");
                    }

                    if (!parent.ChildQuestIds.Contains(quest.Id))
                    {
                        throw new InvalidOperationException(
                            $"Parent quest '{parent.Id}' does not list " +
                            $"'{quest.Id}' as a child.");
                    }
                }

                foreach (string childId in quest.ChildQuestIds)
                {
                    if (!quests.TryGetValue(
                            childId,
                            out QuestDefinition child))
                    {
                        throw new InvalidOperationException(
                            $"Quest '{quest.Id}' references missing child " +
                            $"'{childId}'.");
                    }

                    if (child.ParentQuestId != quest.Id)
                    {
                        throw new InvalidOperationException(
                            $"Child quest '{childId}' has an inconsistent " +
                            $"parent reference.");
                    }
                }

                foreach (string prerequisiteId in quest.PrerequisiteQuestIds)
                {
                    if (!quests.ContainsKey(prerequisiteId))
                    {
                        throw new InvalidOperationException(
                            $"Quest '{quest.Id}' references missing " +
                            $"prerequisite '{prerequisiteId}'.");
                    }

                    if (prerequisiteId == quest.Id)
                    {
                        throw new InvalidOperationException(
                            $"Quest '{quest.Id}' cannot depend on itself.");
                    }
                }
            }

            foreach (KeyValuePair<string, QuestLineDefinition> entry
                     in questLines)
            {
                QuestLineDefinition line = entry.Value;

                foreach (string rootQuestId in line.RootQuestIds)
                {
                    if (!quests.TryGetValue(
                            rootQuestId,
                            out QuestDefinition rootQuest))
                    {
                        throw new InvalidOperationException(
                            $"Quest line '{line.Id}' references missing " +
                            $"root quest '{rootQuestId}'.");
                    }

                    if (!string.IsNullOrWhiteSpace(rootQuest.ParentQuestId))
                    {
                        throw new InvalidOperationException(
                            $"Quest '{rootQuestId}' is listed as a root quest " +
                            "but has a parent.");
                    }
                }
            }

            ValidateNoHierarchyCycles();
        }

        private void ValidateNoHierarchyCycles()
        {
            foreach (QuestDefinition quest in quests.Values)
            {
                var visited = new HashSet<string>();
                string currentId = quest.Id;

                while (!string.IsNullOrWhiteSpace(currentId))
                {
                    if (!visited.Add(currentId))
                    {
                        throw new InvalidOperationException(
                            $"Quest hierarchy contains a cycle at '{currentId}'.");
                    }

                    if (!quests.TryGetValue(
                            currentId,
                            out QuestDefinition current))
                    {
                        break;
                    }

                    currentId = current.ParentQuestId;
                }
            }
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException(
                    "MissionProgressionService has not been initialized.");
            }
        }

        private void EnsureNotInitialized()
        {
            if (isInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot register quest definitions after initialization.");
            }
        }
    }
}
