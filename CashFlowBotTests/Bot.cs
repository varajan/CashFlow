using System.Diagnostics;
using CashFlow.Extensions;

namespace CashFlowBotTests;

public class Bot()
{
    private static readonly string emulatorDirectory = "./../../../../emulator";

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
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Thread.Sleep(100);

            var reply = GetReply(chatId.Value);
            if (lastReply == null && reply != null) return;
            if (lastReply != null && reply.UtcNow > lastReply.UtcNow) return;
        }

        throw new TimeoutException("No reply from bot within the expected time.");
    }

    public static MessageDto GetReply(long chatId)
    {
        var fileName = Path.Combine(emulatorDirectory, $"{chatId}.msg");
        if (!File.Exists(fileName)) return null;

        var lastMessage = File.ReadAllLines(fileName).Last();
        var message = lastMessage.Deserialize<MessageDto>();
        message.Message = message.Message.Trim();

        return message;
    }
}
