using System;
using UnityEngine;

namespace ChainSawLeg.Infrastructure
{
    public sealed class UnityRuntimeErrorLogger : IRuntimeErrorLogger
    {
        private readonly ILogger logger = Debug.unityLogger;

        public void LogException(Exception exception, string context)
        {
            logger.Log(LogType.Error, $"{context}\n{exception}");
        }
    }
}
