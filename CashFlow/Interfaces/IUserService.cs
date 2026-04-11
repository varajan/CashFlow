using CashFlow.Data.DTOs;
using CashFlow.Stages;

namespace CashFlow.Interfaces;

public interface IUserService
{
    bool IsActive(UserDto user);
    Task SetButtons(UserDto user, IStage stage);
    Task Notify(UserDto user, string message);
}
