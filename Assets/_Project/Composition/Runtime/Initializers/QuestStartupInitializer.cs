using System;
using ChainSawLeg.Features.Quest;
using ChainSawLeg.Game.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Composition
{
    public sealed class QuestStartupInitializer: IApplicationService
    {
        private const string QuestLineId = "university_main";
        private const string QuestId = "world_unlock_second_floor";

        private readonly MissionProgressionService questService;

        public QuestStartupInitializer(MissionProgressionService questService)
        {
            this.questService = questService;
        }

        public UniTask Initialize()
        {
            Debug.Log("[QuestStartupInitializer] Initialize started.");
            
            questService.RegisterQuestLine(new QuestLineDefinition(
                id: QuestLineId,
                title: "University",
                type: QuestLineType.Main,
                rootQuestIds: new[] { QuestId }
            ));

            questService.RegisterQuest(new QuestDefinition(
                id: QuestId,
                title: "Conversation with Ksu",
                description: "Talk to Ksu to open a way to the second floor",
                parentQuestId: null,
                childQuestIds: Array.Empty<String>(),
                prerequisiteQuestIds: Array.Empty<String>(),
                completionPolicy: QuestCompletionPolicy.Manual
            ));

            questService.Initialize();

            bool started = questService.StartQuest(QuestId);

            if (!started)
            {
                throw new InvalidOperationException(
                    $"Could not start quest '{QuestId}'. " +
                    $"Current status: {questService.GetStatus(QuestId)}."
                );
            }

            return UniTask.CompletedTask;
        }
        
        public UniTask Dispose()
        {
            return UniTask.CompletedTask;
        }
    }
}