// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class FileGameSaveStorageProvider : IGameSaveStorageProvider
    {
        private const string SaveFileExtension = ".save";

        private readonly IGameSaveSerializer serializer;
        private readonly string rootDirectory;
        private readonly SemaphoreSlim storageGate = new(1, 1);

        public FileGameSaveStorageProvider(IGameSaveSerializer serializer)
            : this(serializer, Path.Combine(Application.persistentDataPath, "GameSaves"))
        {
        }

        public FileGameSaveStorageProvider(IGameSaveSerializer serializer, string rootDirectory)
        {
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Game save root directory is required.", nameof(rootDirectory));

            this.rootDirectory = Path.GetFullPath(rootDirectory);
        }

        public async UniTask WriteAsync(
            GameSaveRequest request,
            byte[] data,
            CancellationToken cancellationToken)
        {
            ValidateRequest(request);

            if (data == null || data.Length == 0)
                throw new GameSaveStorageException("Cannot write empty game save data.");

            string path = BuildPath(request.SlotId);
            string temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
            string backupPath = path + ".backup";

            await storageGate.WaitAsync(cancellationToken);

            try
            {
                await UniTask.RunOnThreadPool(
                    () => WriteAtomically(path, temporaryPath, backupPath, data),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GameSaveStorageException($"Failed to write game save slot: {request.SlotId}.", exception);
            }
            finally
            {
                storageGate.Release();
            }
        }

        public async UniTask<byte[]> ReadAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            string path = BuildPath(request.SlotId);

            await storageGate.WaitAsync(cancellationToken);

            try
            {
                byte[] data = await UniTask.RunOnThreadPool(
                    () => ReadFile(path, request.SlotId),
                    cancellationToken: cancellationToken);

                if (data.Length == 0)
                    throw new CorruptedGameSaveException(request.SlotId);

                DeserializeMetadata(request.SlotId, data);
                return data;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (GameSaveException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GameSaveStorageException($"Failed to read game save slot: {request.SlotId}.", exception);
            }
            finally
            {
                storageGate.Release();
            }
        }

        public async UniTask<IReadOnlyList<GameSaveSlotInfo>> ListSlotsAsync(
            CancellationToken cancellationToken)
        {
            await storageGate.WaitAsync(cancellationToken);

            try
            {
                string[] paths = await UniTask.RunOnThreadPool(
                    ListSavePaths,
                    cancellationToken: cancellationToken);
                var slots = new List<GameSaveSlotInfo>(paths.Length);

                foreach (string path in paths)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string slotId = Path.GetFileNameWithoutExtension(path);

                    if (!IsSafeSlotId(slotId))
                        continue;

                    byte[] data = await UniTask.RunOnThreadPool(
                        () => File.ReadAllBytes(path),
                        cancellationToken: cancellationToken);
                    slots.Add(CreateSlotInfo(slotId, data));
                }

                return slots
                    .OrderBy(slot => slot.IsCorrupted)
                    .ThenByDescending(slot => slot.Metadata?.UtcTimestamp ?? DateTime.MinValue)
                    .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
                    .ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (GameSaveException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GameSaveStorageException("Failed to list game save slots.", exception);
            }
            finally
            {
                storageGate.Release();
            }
        }

        public async UniTask<bool> SlotExistsAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            string path = BuildPath(request.SlotId);

            await storageGate.WaitAsync(cancellationToken);

            try
            {
                return await UniTask.RunOnThreadPool(
                    () => File.Exists(path),
                    cancellationToken: cancellationToken);
            }
            finally
            {
                storageGate.Release();
            }
        }

        public async UniTask DeleteSlotAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            string path = BuildPath(request.SlotId);

            await storageGate.WaitAsync(cancellationToken);

            try
            {
                await UniTask.RunOnThreadPool(
                    () => DeleteSlotFiles(path),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GameSaveStorageException($"Failed to delete game save slot: {request.SlotId}.", exception);
            }
            finally
            {
                storageGate.Release();
            }
        }

        public async UniTask<GameSaveMetadata> ReadMetadataAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken)
        {
            byte[] data = await ReadAsync(request, cancellationToken);
            return DeserializeMetadata(request.SlotId, data);
        }

        private GameSaveSlotInfo CreateSlotInfo(string slotId, byte[] data)
        {
            try
            {
                return new GameSaveSlotInfo
                {
                    SlotId = slotId,
                    Metadata = DeserializeMetadata(slotId, data),
                    IsCorrupted = false
                };
            }
            catch (GameSaveException)
            {
                return new GameSaveSlotInfo
                {
                    SlotId = slotId,
                    Metadata = null,
                    IsCorrupted = true
                };
            }
        }

        private GameSaveMetadata DeserializeMetadata(string slotId, byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new CorruptedGameSaveException(slotId);

            try
            {
                GameSaveData saveData = serializer.Deserialize<GameSaveData>(data);
                GameSaveMetadata metadata = saveData?.Metadata;

                if (metadata == null || metadata.SlotId != slotId)
                    throw new CorruptedGameSaveException(slotId);

                return metadata;
            }
            catch (CorruptedGameSaveException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new CorruptedGameSaveException(slotId, exception);
            }
        }

        private string[] ListSavePaths()
        {
            if (!Directory.Exists(rootDirectory))
                return Array.Empty<string>();

            return Directory.GetFiles(rootDirectory, $"*{SaveFileExtension}", SearchOption.TopDirectoryOnly);
        }

        private string BuildPath(string slotId)
        {
            string path = Path.GetFullPath(Path.Combine(rootDirectory, slotId + SaveFileExtension));
            string expectedPrefix = rootDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!path.StartsWith(expectedPrefix, StringComparison.Ordinal))
                throw new GameSaveValidationException("Game save SlotId resolves outside the save directory.");

            return path;
        }

        private static void ValidateRequest(GameSaveRequest request)
        {
            if (request == null || !IsSafeSlotId(request.SlotId))
                throw new GameSaveValidationException("Game save SlotId is invalid.");
        }

        private static bool IsSafeSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId) || slotId.Length > 64 ||
                Path.IsPathRooted(slotId) || slotId.Contains(".."))
                return false;

            foreach (char character in slotId)
            {
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '-')
                    return false;
            }

            return true;
        }

        private static void WriteAtomically(
            string path,
            string temporaryPath,
            string backupPath,
            byte[] data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            try
            {
                using (var stream = new FileStream(
                           temporaryPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           4096,
                           FileOptions.WriteThrough))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush(true);
                }

                if (!File.Exists(path))
                {
                    File.Move(temporaryPath, path);
                    return;
                }

                if (File.Exists(backupPath))
                    File.Delete(backupPath);

                try
                {
                    File.Replace(temporaryPath, path, backupPath);
                }
                catch (PlatformNotSupportedException)
                {
                    ReplaceWithRollback(path, temporaryPath, backupPath);
                }

                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private static void ReplaceWithRollback(string path, string temporaryPath, string backupPath)
        {
            File.Move(path, backupPath);

            try
            {
                File.Move(temporaryPath, path);
            }
            catch
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.Move(backupPath, path);
                throw;
            }
        }

        private static byte[] ReadFile(string path, string slotId)
        {
            if (!File.Exists(path))
                throw new GameSaveStorageException($"Game save slot does not exist: {slotId}.");

            return File.ReadAllBytes(path);
        }

        private static void DeleteSlotFiles(string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            string backupPath = path + ".backup";

            if (File.Exists(backupPath))
                File.Delete(backupPath);

            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                return;

            foreach (string temporaryPath in Directory.GetFiles(
                         directory,
                         Path.GetFileName(path) + ".*.tmp",
                         SearchOption.TopDirectoryOnly))
                File.Delete(temporaryPath);
        }
    }
}
