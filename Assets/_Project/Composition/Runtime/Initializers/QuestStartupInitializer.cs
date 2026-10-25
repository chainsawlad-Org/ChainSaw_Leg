
using System;
using ChainSawLeg.Features.Quest;
using ChainSawLeg.Game.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Composition
{
    public sealed class QuestStartupInitializer : IApplicationService
    {
        private readonly QuestDatabaseConfig questDatabaseConfig;
        private readonly MissionProgressionService questService;

        public QuestStartupInitializer(
            QuestDatabaseConfig questDatabaseConfig,
            MissionProgressionService questService)
        {
            this.questDatabaseConfig = questDatabaseConfig;
            this.questService = questService;
        }

        public UniTask Initialize()
        {
            if (questDatabaseConfig == null)
            {
                throw new InvalidOperationException(
                    "QuestDatabaseConfig is not available.");
            }

            foreach (QuestDatabaseConfig.QuestLineEntry entry
                     in questDatabaseConfig.QuestLines)
            {
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        "QuestDatabaseConfig contains a null quest line.");
                }

                questService.RegisterQuestLine(
                    new QuestLineDefinition(
                        id: entry.id,
                        title: entry.title,
                        type: entry.type,
                        rootQuestIds: entry.rootQuestIds
                    )
                );
            }

            foreach (QuestDatabaseConfig.QuestEntry entry
                     in questDatabaseConfig.Quests)
            {
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        "QuestDatabaseConfig contains a null quest.");
                }

                questService.RegisterQuest(
                    new QuestDefinition(
                        id: entry.id,
                        title: entry.title,
                        description: entry.description,
                        parentQuestId: string.IsNullOrWhiteSpace(
                            entry.parentQuestId)
                            ? null
                            : entry.parentQuestId,
                        childQuestIds: entry.childQuestIds,
                        prerequisiteQuestIds: entry.prerequisiteQuestIds,
                        completionPolicy: entry.completionPolicy
                    )
                );
            }

            questService.Initialize();

            foreach (QuestDatabaseConfig.QuestEntry entry
                     in questDatabaseConfig.Quests)
            {
                if (!entry.startOnGameStart)
                {
                    continue;
                }

                bool started = questService.StartQuest(entry.id);

                if (!started)
                {
                    throw new InvalidOperationException(
                        $"Could not start quest '{entry.id}'. " +
                        $"Current status: {questService.GetStatus(entry.id)}.");
                }
            }

            Debug.Log(
                $"[QuestStartupInitializer] Initialized " +
                $"{questDatabaseConfig.QuestLines.Count} quest lines and " +
                $"{questDatabaseConfig.Quests.Count} quests.");

            return UniTask.CompletedTask;
        }

        public UniTask Dispose()
        {
            return UniTask.CompletedTask;
        }
    }
}
