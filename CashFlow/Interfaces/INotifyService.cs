using CashFlow.Stages;

namespace CashFlow.Interfaces;

public interface INotifyService
{
    Task SetButtons(long userId, IStage stage);
    Task Notify(long userId, string message);
}