using System;
using System.Collections.Generic;

namespace CashFlow.Loggers;

public interface ILogger
{
    void Log(Exception exception);
    void Log(string message);
    IList<string> GetTop30Records();
}
