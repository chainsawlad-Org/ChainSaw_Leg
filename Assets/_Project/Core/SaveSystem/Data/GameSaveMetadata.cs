using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveMetadata
    {
        public int FormatVersion;
        public DateTime UtcTimestamp;
        public string BuildNumber;
        public string ProfileId;
        public GameSaveKind Kind;
        public string SlotId;

        public static GameSaveMetadata Create(
            GameSaveRequest request,
            int formatVersion,
            DateTime utcTimestamp,
            string buildNumber,
            string profileId)
        {
            return new GameSaveMetadata
            {
                FormatVersion = formatVersion,
                UtcTimestamp = utcTimestamp.Kind == DateTimeKind.Utc
                    ? utcTimestamp
                    : utcTimestamp.ToUniversalTime(),
                BuildNumber = buildNumber,
                ProfileId = profileId,
                Kind = request.Kind,
                SlotId = request.SlotId
            };
        }
    }
}
