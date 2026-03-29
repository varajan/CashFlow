using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
using CashFlow.Stages;

namespace CashFlow.Extensions;

public static class UserExtensions
{
    public static bool IsActive(this UserDto user)
    {
        var personRepository = ServicesProvider.Get<IPersonRepository>();
        return personRepository.Get(user.Id)?.LastActive > DateTime.Now.AddMinutes(-15);
    }

    public static async Task SetButtons(this UserDto user, IStage stage)
    {
        var notifyService = ServicesProvider.Get<INotifyService>();
        await notifyService.SetButtons(user.Id, stage);
    }

    public static async Task Notify(this UserDto user, string message)
    {
        var notifyService = ServicesProvider.Get<INotifyService>();
        await notifyService.Notify(user.Id, message);
    }
}
