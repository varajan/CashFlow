using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Polly;
using Polly.Retry;

namespace CashFlowBotEmulator;

public class EmulationNotifyService(long chatId) : INotifyService
{
    private string FileName => $"{chatId}.msg";

    public Task SetButtons(IStage stage)
    {
        var @object = new { stage.Message, stage.Buttons, DateTime.UtcNow };
        File.AppendAllText(FileName, $"\n{@object.Serialize()}");
        return Task.CompletedTask;
    }

    public Task Notify(string message)
    {
        var @object = new { message, DateTime.UtcNow };
        var appendFile = () =>
        {
            File.AppendAllText(FileName, $"\n{@object.Serialize()}");
            return Task.CompletedTask;
        };

        return _retryPolicy.ExecuteAsync(appendFile);
    }

    private static readonly AsyncRetryPolicy _retryPolicy =
        Policy
            .Handle<IOException>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(200 * retryAttempt)
            );
}
