using System;
using System.Collections.Generic;

namespace ChainSawLeg.Features.Quest
{
    public sealed class QuestDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }

        public string ParentQuestId { get; }

        public IReadOnlyList<string> ChildQuestIds { get; }
        public IReadOnlyList<string> PrerequisiteQuestIds { get; }

        public QuestCompletionPolicy CompletionPolicy { get; }

        public QuestDefinition(
            string id,
            string title,
            string description,
            string parentQuestId,
            IReadOnlyList<string> childQuestIds,
            IReadOnlyList<string> prerequisiteQuestIds,
            QuestCompletionPolicy completionPolicy)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Quest ID cannot be empty.", nameof(id));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Quest title cannot be empty.", nameof(title));

            Id = id;
            Title = title;
            Description = description ?? string.Empty;
            ParentQuestId = parentQuestId;

            ChildQuestIds = CopyIds(childQuestIds);
            PrerequisiteQuestIds = CopyIds(prerequisiteQuestIds);

            CompletionPolicy = completionPolicy;
        }

        private static IReadOnlyList<string> CopyIds(
            IReadOnlyList<string> ids)
        {
            var result = new List<string>();

            if (ids != null)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(ids[i]))
                        result.Add(ids[i]);
                }
            }

            return result.AsReadOnly();
        }
    }
}