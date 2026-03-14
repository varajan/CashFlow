using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages;
using Polly;
using Polly.Retry;

namespace CashFlowBotEmulator;

public class EmulationNotifyService : INotifyService
{
    private static string FileName(long userId) => $"{userId}.msg";

    public Task SetButtons(long userId, IStage stage)
    {
        Task setButtons()
        {
            var fileName = FileName(userId);
            var @object = new { stage.Message, stage.Buttons, DateTime = DateTime.UtcNow };
            File.AppendAllText(fileName, $"\n{@object.Serialize()}");
            return Task.CompletedTask;
        }

        return _retryPolicy.ExecuteAsync(setButtons);
    }

    public Task Notify(long userId, string message)
    {
        Task notify()
        {
            var fileName = FileName(userId);
            var @object = new { Message = message, DateTime = DateTime.UtcNow };
            File.AppendAllText(fileName, $"\n{@object.Serialize()}");
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
