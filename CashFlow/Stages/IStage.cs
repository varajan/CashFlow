using CashFlow.Data.Users;

namespace CashFlow.Stages;

public interface IStage
{
    IUser CurrentUser { get; }
    string Name { get; }
    string Message { get; }
    IEnumerable<string> Buttons { get; }
    IStage NextStage { get; }
    IStage SetCurrentUser(IUser user);
    IStage SetAllUsers(IList<IUser> users);

    Task HandleMessage(string message);
    Task SetButtons();
}