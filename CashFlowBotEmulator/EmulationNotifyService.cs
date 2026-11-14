using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;

namespace CashFlowBotEmulator;

public class EmulationNotifyService : INotifyService
{
    public Task SetButtons(long userId, IStage stage)
    {
        var @object = new
        {
            stage.Message,
            Buttons = stage.ButtonsAsList.Any() ? stage.ButtonsAsList : stage.ButtonsAsMatrix.SelectMany(x => x),
            DateTime = DateTime.UtcNow
        };

        Console.WriteLine($"<- {userId} {@object.Serialize()}");
        return Task.CompletedTask;
    }

    public Task Notify(long userId, string message)
    {
        var @object = new { Message = message, DateTime = DateTime.UtcNow };
        Console.WriteLine($"<- {userId} {@object.Serialize()}");
        return Task.CompletedTask;
    }
}
