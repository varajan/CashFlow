using System.Diagnostics;
using System.Runtime.InteropServices;
using CashFlow.Extensions;
using Polly;
using Polly.Retry;

namespace CashFlowBotTests.Extras;

public class Bot()
{
    private static readonly string emulatorDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "./../../../../CashFlowBotEmulator/emulator"
        : "./../../../../emulator";

    public static void SendMessage(string message, long? chatId = null)
    {
        var lastReply = chatId.HasValue ? GetReply(chatId.Value) : null;
        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{chatId}";
        var tmpFile = Path.Combine(emulatorDirectory, $"{fileName}.txt");
        var finalFile = Path.Combine(emulatorDirectory, $"{fileName}.cmd");

        File.WriteAllText(tmpFile, message);
        File.Move(tmpFile, finalFile);
        WaitForReply(chatId, lastReply);
    }

    private static void WaitForReply(long? chatId, MessageDto lastReply)
    {
        if (!chatId.HasValue) return;

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Thread.Sleep(100);

            var reply = GetReply(chatId.Value);
            if (lastReply == null && reply != null) return;
            if (lastReply != null && reply.DateTime > lastReply.DateTime) return;
        }

        throw new TimeoutException($"{DateTime.UtcNow} [{chatId}] No reply from bot within the expected time.");
    }

    public static MessageDto GetReply(long chatId, int indexFromEnd = 0)
    {
        var fileName = Path.Combine(emulatorDirectory, $"{chatId}.msg");
        var getReply = () =>
        {
            var lines = File.ReadAllLines(fileName);
            var lastMessage = lines[^(indexFromEnd + 1)];
            var message = lastMessage.Deserialize<MessageDto>();
            message.Message = message?.Message?.Trim();

            return message;
        };

        return File.Exists(fileName)
            ? _retryPolicy.Execute(getReply)
            : null;
    }

    private static readonly RetryPolicy _retryPolicy =
        Policy
            .Handle<IOException>()
            .WaitAndRetry(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(200 * retryAttempt));
}
