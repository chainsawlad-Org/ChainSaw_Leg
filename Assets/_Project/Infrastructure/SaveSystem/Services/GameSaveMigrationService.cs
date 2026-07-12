// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using System.Collections.Generic;
using System.Linq;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSaveMigrationService
    {
        private readonly IReadOnlyList<IGameSaveMigrationStep> migrationSteps;

        public GameSaveMigrationService(List<IGameSaveMigrationStep> migrationSteps)
        {
            this.migrationSteps = migrationSteps ?? new List<IGameSaveMigrationStep>();
        }

        public GameSaveData Migrate(GameSaveData saveData)
        {
            if (saveData?.Metadata == null)
                throw new GameSaveMigrationException("Cannot migrate game save data without metadata.");

            int version = saveData.Metadata.FormatVersion;

            if (version > GameSaveData.CurrentFormatVersion || version < 0)
                throw new UnknownGameSaveVersionException(version);

            while (version < GameSaveData.CurrentFormatVersion)
            {
                List<IGameSaveMigrationStep> matchingSteps = migrationSteps
                    .Where(step => step.SourceVersion == version)
                    .ToList();

                if (matchingSteps.Count != 1)
                    throw new GameSaveMigrationException($"Missing migration from game save version {version}.");

                IGameSaveMigrationStep migrationStep = matchingSteps[0];

                if (migrationStep.TargetVersion <= version ||
                    migrationStep.TargetVersion > GameSaveData.CurrentFormatVersion)
                    throw new GameSaveMigrationException($"Invalid migration target version: {migrationStep.TargetVersion}.");

                saveData = migrationStep.Migrate(saveData);

                if (saveData?.Metadata == null)
                    throw new GameSaveMigrationException("Migration returned invalid game save data.");

                version = migrationStep.TargetVersion;
                saveData.Metadata.FormatVersion = version;
            }

            return saveData;
        }
    }
}
