using CashFlow.Interfaces;

namespace CashFlow.Stages;

public interface IStage
{
    ICashFlowUser CurrentUser { get; }
    string Name { get; }
    string Message { get; }
    IEnumerable<string> Buttons { get; }
    IStage NextStage { get; }
    IStage SetCurrentUser(ICashFlowUser user);
    IStage SetAllUsers(IList<ICashFlowUser> users);

    Task BeforeStage();
    Task HandleMessage(string message);
    Task SetButtons();
}