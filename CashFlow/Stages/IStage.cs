using CashFlow.Data.DTOs;

namespace CashFlow.Stages;

public interface IStage
{
    UserDto CurrentUser { get; }
    string Name { get; }
    string Message { get; }
    IEnumerable<string> Buttons { get; }
    IStage NextStage { get; }
    IStage SetCurrentUser(UserDto user);

    Task BeforeStage();
    Task HandleMessage(string message);
    Task SetButtons();
}