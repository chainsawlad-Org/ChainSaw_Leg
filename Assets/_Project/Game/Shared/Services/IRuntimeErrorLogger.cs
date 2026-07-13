using System;

public interface IRuntimeErrorLogger
{
    void LogException(Exception exception, string context);
}
