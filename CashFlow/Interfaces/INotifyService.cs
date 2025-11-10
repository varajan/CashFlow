using CashFlow.Stages;

namespace CashFlow.Interfaces;

public interface INotifyService
{
    Task SetButtons(IStage stage);
    Task Notify(string message);
}