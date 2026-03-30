using System.Diagnostics;
using CashFlow.Extensions;
using MoreLinq;
using Polly;
using Polly.Retry;

namespace CashFlowBotSystemTests.Extras;

public class Bot()
{
    private static readonly string root = Path.GetFullPath("./../../../..");
    private static readonly string projectPath = Path.Combine(root, "CashFlowBotEmulator", "CashFlowBotEmulator.csproj");
    private static readonly string emulatorDirectory = Path.Combine(root, "emulator");

    public static void Launch()
    {
        CleanEmulatorDirectory();
        SendMessage("CHECK");
        var reply = GetReply(0);
        if (reply is not null) return;

        BuildEmulator();
        StartEmulator();
    }

    public static void Close() => SendMessage("EXIT");

    private static void CleanEmulatorDirectory()
    {
        var extensions = new[] { ".msg", ".cmd" };

        Directory.CreateDirectory(emulatorDirectory);
        Directory
            .EnumerateFiles(emulatorDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .ForEach(File.Delete);
    }

    private static void BuildEmulator()
    {
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = $"publish {projectPath} -c Release -r win-x64 --self-contained true -o {emulatorDirectory}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception("Publish failed");
        }
    }

    private static void StartEmulator()
    {
        var exePath = Path.Combine(emulatorDirectory, "CashFlowBotEmulator.exe");
        var process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.WorkingDirectory = emulatorDirectory;
        process.Start();
    }

    public static void SendMessage(string message, long? chatId = null)
    {
        var lastReply = chatId.HasValue ? GetReply(chatId.Value) : null;
        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{chatId}";
        var tmpFile = Path.Combine(emulatorDirectory, $"{fileName}.txt");
        var finalFile = Path.Combine(emulatorDirectory, $"{fileName}.cmd");

        File.WriteAllText(tmpFile, message);
        File.Move(tmpFile, finalFile);
        WaitForReply(chatId, lastReply);
        Thread.Sleep(100);
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
