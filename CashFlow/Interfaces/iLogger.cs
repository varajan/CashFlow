namespace CashFlow.Interfaces;

public interface ILogger
{
    void Log(Exception exception);
    void Log(string message);
    IList<string> GetTop30Records();
}
