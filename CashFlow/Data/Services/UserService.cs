using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;

namespace CashFlow.Data.Services;

public class UserService(INotifyService notifyService, IPersonRepository personRepository) : IUserService
{
    public bool IsActive(UserDto user) => personRepository.Get(user.Id)?.LastActive > DateTime.Now.AddMinutes(-15);

    public async Task SetButtons(UserDto user, IStage stage) => await notifyService.SetButtons(user.Id, stage);

    public async Task Notify(UserDto user, string message) => await notifyService.Notify(user.Id, message);
}
