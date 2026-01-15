using System.Diagnostics;
using CashFlow.Extensions;

namespace CashFlowBotTests;

public class Bot()
{
    private static readonly string emulatorDirectory = "./../../../../../emulator";

    public static void SendMessage(string message, long? chatId = null)
    {
        var lastReply = chatId.HasValue ? GetReply(chatId.Value) : null;
        var file = Path.Combine(emulatorDirectory, $"{DateTime.UtcNow.Ticks}_{chatId}.cmd");
        File.WriteAllText(file, message);
        WaitForReply(chatId, lastReply);
    }

    private static void WaitForReply(long? chatId, MessageDto lastReply)
    {
        if (!chatId.HasValue) return;

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            var reply = GetReply(chatId.Value);
            if (lastReply == null && reply != null) return;
            if (lastReply != null && reply.UtcNow > lastReply.UtcNow) return;
            Thread.Sleep(100);
        }

        throw new TimeoutException("No reply from bot within the expected time.");
    }

    private static string GetFileName(long chatId) => Path.Combine(emulatorDirectory, $"{chatId}.msg");

    public static MessageDto GetReply(long chatId)
    {
        var fileName = Path.Combine(emulatorDirectory, GetFileName(chatId));
        if (!File.Exists(fileName)) return null;

        var lastMessage = File.ReadAllLines(fileName).Last();
        var message = lastMessage.Deserialize<MessageDto>();
        message.Message = message.Message.Trim();

        return message;
    }
}
