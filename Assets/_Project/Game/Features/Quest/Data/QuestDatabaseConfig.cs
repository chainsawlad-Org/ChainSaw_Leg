using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Quest
{
    [CreateAssetMenu(
        fileName = "QuestDatabaseConfig",
        menuName = "ChainSawLeg/Quest/Database Config")]
    public sealed class QuestDatabaseConfig : ScriptableObject
    {
        [SerializeField]
        private List<QuestLineEntry> questLines = new();

        [SerializeField]
        private List<QuestEntry> quests = new();

        public IReadOnlyList<QuestLineEntry> QuestLines => questLines;
        public IReadOnlyList<QuestEntry> Quests => quests;

        [Serializable]
        public sealed class QuestLineEntry
        {
            public string id;
            public string title;
            public QuestLineType type;
            public List<string> rootQuestIds = new();
        }

        [Serializable]
        public sealed class QuestEntry
        {
            public string id;
            public string title;
            [TextArea]
            public string description;

            public string parentQuestId;
            public List<string> childQuestIds = new();
            public List<string> prerequisiteQuestIds = new();

            public QuestCompletionPolicy completionPolicy;
            public bool startOnGameStart;
        }
    }
}