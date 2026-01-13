using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;

namespace CashFlowBotEmulator;

public class EmulationNotifyService(long chatId) : INotifyService
{
    private string FileName => $"EmulationChat_{chatId}.txt";

    public Task SetButtons(IStage stage)
    {
        var @object = new { stage.Message, stage.Buttons, DateTime.UtcNow };
        File.AppendAllText(FileName, $"\n{@object.Serialize()}");
        return Task.CompletedTask;
    }

    public Task Notify(string message)
    {
        var @object = new { message, DateTime.UtcNow };
        File.AppendAllText(FileName, $"\n{@object.Serialize()}"); return Task.CompletedTask;
    }
}
