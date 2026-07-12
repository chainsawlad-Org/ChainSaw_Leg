// Placement: Docs/Ru/02_ProjectStructure.md:172-176. Quote: "Любой код, работающий непосредственно с API Unity, должен находиться здесь."

using System;
using UnityEngine;

public sealed class UnityRuntimeErrorLogger : IRuntimeErrorLogger
{
    private readonly ILogger logger = Debug.unityLogger;

    public void LogException(Exception exception, string context)
    {
        logger.Log(LogType.Error, $"{context}\n{exception}");
    }
}
