using System;
using System.Collections.Generic;

namespace ChainSawLeg.Features.Quest
{
    public sealed class QuestLineDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public QuestLineType Type { get; }

        public IReadOnlyList<string> RootQuestIds { get; }

        public QuestLineDefinition(
            string id,
            string title,
            QuestLineType type,
            IReadOnlyList<string> rootQuestIds)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Quest line ID cannot be empty.", nameof(id));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Quest line title cannot be empty.", nameof(title));

            Id = id;
            Title = title;
            Type = type;

            var result = new List<string>();

            if (rootQuestIds != null)
            {
                for (int i = 0; i < rootQuestIds.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(rootQuestIds[i]))
                        result.Add(rootQuestIds[i]);
                }
            }

            RootQuestIds = result.AsReadOnly();
        }
    }
}