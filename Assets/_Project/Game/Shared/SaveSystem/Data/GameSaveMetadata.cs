using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveMetadata
    {
        private GameSaveMetadata()
        {
        }

        public GameSaveMetadata(
            int formatVersion,
            DateTime utcTimestamp,
            string buildNumber,
            string profileId,
            GameSaveKind kind,
            string slotId)
        {
            FormatVersion = formatVersion;
            UtcTimestamp = utcTimestamp.Kind == DateTimeKind.Utc
                ? utcTimestamp
                : utcTimestamp.ToUniversalTime();
            BuildNumber = buildNumber;
            ProfileId = profileId;
            Kind = kind;
            SlotId = slotId;
        }

        public int FormatVersion { get; private set; }
        public DateTime UtcTimestamp { get; private set; }
        public string BuildNumber { get; private set; }
        public string ProfileId { get; private set; }
        public GameSaveKind Kind { get; private set; }
        public string SlotId { get; private set; }

        public static GameSaveMetadata Create(
            GameSaveRequest request,
            int formatVersion,
            DateTime utcTimestamp,
            string buildNumber,
            string profileId)
        {
            return new GameSaveMetadata(
                formatVersion,
                utcTimestamp,
                buildNumber,
                profileId,
                request.Kind,
                request.SlotId);
        }

        public GameSaveMetadata WithFormatVersion(int formatVersion)
        {
            return new GameSaveMetadata(
                formatVersion,
                UtcTimestamp,
                BuildNumber,
                ProfileId,
                Kind,
                SlotId);
        }
    }
}
