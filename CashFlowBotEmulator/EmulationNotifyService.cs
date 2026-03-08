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
        Task setButtons()
        {
            var @object = new { stage.Message, stage.Buttons, DateTime = DateTime.UtcNow };
            File.AppendAllText(FileName, $"\n{@object.Serialize()}");
            return Task.CompletedTask;
        }

        return _retryPolicy.ExecuteAsync(setButtons);
    }

    public Task Notify(string message)
    {
        Task notify()
        {
            var @object = new { Message = message, DateTime = DateTime.UtcNow };
            File.AppendAllText(FileName, $"\n{@object.Serialize()}");
            return Task.CompletedTask;
        }

        return _retryPolicy.ExecuteAsync(notify);
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
