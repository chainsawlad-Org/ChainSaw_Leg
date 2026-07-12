// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSaveValidationService
    {
        public void ValidateRequest(GameSaveRequest request)
        {
            if (request == null)
                throw new GameSaveValidationException("Game save request is required.");

            if (string.IsNullOrWhiteSpace(request.SlotId))
                throw new GameSaveValidationException("Game save SlotId is required.");

            if (request.SlotId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                request.SlotId.Contains("/") ||
                request.SlotId.Contains("\\"))
                throw new GameSaveValidationException("Game save SlotId contains invalid characters.");
        }

        public void ValidateForSave(GameSaveData saveData)
        {
            ValidateSnapshot(saveData);

            if (saveData.Metadata.FormatVersion != GameSaveData.CurrentFormatVersion)
                throw new UnknownGameSaveVersionException(saveData.Metadata.FormatVersion);
        }

        public void ValidateLoadedData(GameSaveData saveData)
        {
            ValidateSnapshot(saveData);

            if (saveData.Metadata.FormatVersion != GameSaveData.CurrentFormatVersion)
                throw new UnknownGameSaveVersionException(saveData.Metadata.FormatVersion);
        }

        public void ValidateContributorData(object saveData, Type expectedType)
        {
            if (saveData == null)
                throw new GameSaveValidationException("Contributor save DTO cannot be null.");

            if (expectedType == null || !expectedType.IsInstanceOfType(saveData))
                throw new GameSaveValidationException("Contributor save DTO type does not match its registration.");

            ValidateObjectGraph(saveData, new HashSet<object>(new ReferenceComparer()));
        }

        private static void ValidateSnapshot(GameSaveData saveData)
        {
            if (saveData == null)
                throw new GameSaveValidationException("Game save data is required.");

            GameSaveMetadata metadata = saveData.Metadata;

            if (metadata == null)
                throw new GameSaveValidationException("Game save metadata is required.");

            if (string.IsNullOrWhiteSpace(metadata.SlotId))
                throw new GameSaveValidationException("Game save metadata SlotId is required.");

            if (string.IsNullOrWhiteSpace(metadata.BuildNumber))
                throw new GameSaveValidationException("Game save build number is required.");

            if (string.IsNullOrWhiteSpace(metadata.ProfileId))
                throw new GameSaveValidationException("Game save profile ID is required.");

            if (metadata.UtcTimestamp.Kind != DateTimeKind.Utc)
                throw new GameSaveValidationException("Game save timestamp must be UTC.");

            if (saveData.Entries == null)
                throw new GameSaveValidationException("Game save entries collection is required.");

            var contributorIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (GameSaveEntry entry in saveData.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ContributorId))
                    throw new GameSaveValidationException("Game save entry contributor ID is required.");

                if (!contributorIds.Add(entry.ContributorId))
                    throw new GameSaveValidationException($"Duplicate game save contributor ID: {entry.ContributorId}.");

                if (entry.Payload == null || entry.Payload.Length == 0)
                    throw new GameSaveValidationException($"Game save entry payload is empty: {entry.ContributorId}.");
            }
        }

        private static void ValidateObjectGraph(object value, HashSet<object> visited)
        {
            if (value == null)
                return;

            Type type = value.GetType();

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                throw new GameSaveValidationException($"Unity object references are forbidden in save DTO: {type.FullName}.");

            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) ||
                type == typeof(DateTime) || type == typeof(Guid) || type == typeof(TimeSpan))
                return;

            if (!type.IsValueType && !visited.Add(value))
                return;

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                    ValidateObjectGraph(item, visited);

                return;
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (!field.IsStatic)
                    ValidateObjectGraph(field.GetValue(value), visited);
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
