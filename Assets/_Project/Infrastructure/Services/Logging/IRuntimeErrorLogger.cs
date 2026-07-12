// Placement: Docs/Ru/02_ProjectStructure.md:172-176. Quote: "Любой код, работающий непосредственно с API Unity, должен находиться здесь."

using System;

public interface IRuntimeErrorLogger
{
    void LogException(Exception exception, string context);
}
