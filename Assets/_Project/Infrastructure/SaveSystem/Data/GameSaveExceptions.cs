// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public class GameSaveException : Exception
    {
        public GameSaveException(string message) : base(message)
        {
        }

        public GameSaveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class GameSaveValidationException : GameSaveException
    {
        public GameSaveValidationException(string message) : base(message)
        {
        }
    }

    public sealed class GameSaveSerializationException : GameSaveException
    {
        public GameSaveSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class GameSaveStorageException : GameSaveException
    {
        public GameSaveStorageException(string message) : base(message)
        {
        }

        public GameSaveStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class CorruptedGameSaveException : GameSaveException
    {
        public CorruptedGameSaveException(string slotId)
            : base($"Game save slot is empty or corrupted: {slotId}.")
        {
        }

        public CorruptedGameSaveException(string slotId, Exception innerException)
            : base($"Game save slot is empty or corrupted: {slotId}.", innerException)
        {
        }
    }

    public sealed class UnknownGameSaveVersionException : GameSaveException
    {
        public UnknownGameSaveVersionException(int version)
            : base($"Unsupported game save format version: {version}.")
        {
        }
    }

    public sealed class GameSaveMigrationException : GameSaveException
    {
        public GameSaveMigrationException(string message) : base(message)
        {
        }
    }
}
