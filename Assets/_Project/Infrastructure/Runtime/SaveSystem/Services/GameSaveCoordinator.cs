using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSaveCoordinator
    {
        private readonly IGameSaveSerializer serializer;
        private readonly IGameSaveStorageProvider storageProvider;
        private readonly GameSaveValidationService validationService;
        private readonly GameSaveMigrationService migrationService;
        private readonly IReadOnlyList<IGameSaveContributor> contributors;
        private readonly IReadOnlyList<IGameSaveRestorer> restorers;

        public GameSaveCoordinator(
            IGameSaveSerializer serializer,
            IGameSaveStorageProvider storageProvider,
            GameSaveValidationService validationService,
            GameSaveMigrationService migrationService,
            List<IGameSaveContributor> contributors,
            List<IGameSaveRestorer> restorers)
        {
            this.serializer = serializer;
            this.storageProvider = storageProvider;
            this.validationService = validationService;
            this.migrationService = migrationService;
            this.contributors = contributors ?? new List<IGameSaveContributor>();
            this.restorers = restorers ?? new List<IGameSaveRestorer>();
        }

        public async UniTask SaveAsync(
            GameSaveRequest request,
            string profileId,
            string buildNumber,
            CancellationToken cancellationToken)
        {
            validationService.ValidateRequest(request);
            cancellationToken.ThrowIfCancellationRequested();

            GameSaveMetadata metadata = GameSaveMetadata.Create(
                request,
                GameSaveData.CurrentFormatVersion,
                DateTime.UtcNow,
                buildNumber,
                profileId);
            var entries = new List<GameSaveEntry>();

            var contributorIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (IGameSaveContributor contributor in contributors.OrderBy(item => item.ContributorId))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateContributorRegistration(contributor, contributorIds);

                object contributorData = contributor.CaptureSaveData();
                validationService.ValidateContributorData(contributorData, contributor.SaveDataType);

                entries.Add(new GameSaveEntry(
                    contributor.ContributorId,
                    serializer.Serialize(contributorData, contributor.SaveDataType)));
            }

            var saveData = new GameSaveData(metadata, entries);
            validationService.ValidateForSave(saveData);
            byte[] serializedSnapshot = serializer.Serialize(saveData);
            GameSaveData verifiedSnapshot = serializer.Deserialize<GameSaveData>(serializedSnapshot);
            validationService.ValidateLoadedData(verifiedSnapshot);

            if (verifiedSnapshot.Metadata.SlotId != request.SlotId ||
                verifiedSnapshot.Metadata.Kind != request.Kind)
            {
                throw new GameSaveValidationException(
                    "Serialized game save identity does not match the requested slot.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            await storageProvider.WriteAsync(request, serializedSnapshot, cancellationToken);
        }

        public async UniTask<GameSaveData> LoadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            GameSaveData saveData = await ReadAsync(request, cancellationToken);
            Restore(saveData, cancellationToken);
            return saveData;
        }

        public async UniTask<GameSaveData> ReadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            validationService.ValidateRequest(request);
            byte[] serializedSnapshot = await storageProvider.ReadAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            GameSaveData saveData = serializer.Deserialize<GameSaveData>(serializedSnapshot);
            saveData = migrationService.Migrate(saveData);
            validationService.ValidateLoadedData(saveData);

            return saveData;
        }

        public T ReadContributorData<T>(GameSaveData saveData, string contributorId)
        {
            validationService.ValidateLoadedData(saveData);
            GameSaveEntry entry = saveData.Entries.SingleOrDefault(
                item => item.ContributorId == contributorId);

            if (entry == null)
                throw new GameSaveValidationException($"Game save contributor data was not found: {contributorId}.");

            T contributorData = serializer.Deserialize<T>(entry.Payload);
            validationService.ValidateContributorData(contributorData, typeof(T));
            return contributorData;
        }

        public void Restore(GameSaveData saveData, CancellationToken cancellationToken)
        {
            validationService.ValidateLoadedData(saveData);

            Dictionary<string, IGameSaveRestorer> restorersById = BuildRestorerMap();

            foreach (GameSaveEntry entry in saveData.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!restorersById.TryGetValue(entry.ContributorId, out IGameSaveRestorer restorer))
                    continue;

                object contributorData = serializer.Deserialize(entry.Payload, restorer.SaveDataType);
                validationService.ValidateContributorData(contributorData, restorer.SaveDataType);
                restorer.RestoreSaveData(contributorData);
            }
        }

        private static void ValidateContributorRegistration(
            IGameSaveContributor contributor,
            HashSet<string> contributorIds)
        {
            if (contributor == null || string.IsNullOrWhiteSpace(contributor.ContributorId))
                throw new GameSaveValidationException("Registered game save contributor ID is required.");

            if (contributor.SaveDataType == null)
                throw new GameSaveValidationException($"Contributor save DTO type is required: {contributor.ContributorId}.");

            if (!contributorIds.Add(contributor.ContributorId))
                throw new GameSaveValidationException($"Duplicate game save contributor ID: {contributor.ContributorId}.");
        }

        private Dictionary<string, IGameSaveRestorer> BuildRestorerMap()
        {
            var result = new Dictionary<string, IGameSaveRestorer>(StringComparer.Ordinal);

            foreach (IGameSaveRestorer restorer in restorers)
            {
                if (restorer == null || string.IsNullOrWhiteSpace(restorer.ContributorId) || restorer.SaveDataType == null)
                    throw new GameSaveValidationException("Registered game save restorer is invalid.");

                if (!result.TryAdd(restorer.ContributorId, restorer))
                    throw new GameSaveValidationException($"Duplicate game save restorer ID: {restorer.ContributorId}.");
            }

            return result;
        }
    }
}
