using CashFlow.Extensions;
using MoreLinq;
using Polly;
using Polly.Retry;
using System.Diagnostics;

namespace CashFlowBotSystemTests.Extras;

public class Bot(string folder)
{
    private static string Root => Path.GetFullPath("./../../../..");
    private static string ProjectPath => Path.Combine(Root, "CashFlowBotEmulator", "CashFlowBotEmulator.csproj");
    private string EmulatorDirectory => Path.Combine(Root, "emulator", folder.Replace(" ", ""));

    private static readonly Mutex _lock = new(false, "LOCK");

    public void Launch()
    {
        CleanEmulatorDirectory();
        SendMessage("CHECK");
        var reply = GetReply(0);
        if (reply is not null) return;

        _lock.WaitOne();
        try
        {
            BuildEmulator();
            StartEmulator();
        }
        finally
        {
            _lock.ReleaseMutex();
        }
    }

    public void Close()
    {
        SendMessage("EXIT");
        CleanEmulatorDirectory(".dll", ".pdb", ".json");
    }

    private void CleanEmulatorDirectory(params string[] extensions)
    {
        string[] subFolders = [ "Data", "runtimes" ];
        extensions = extensions.Length > 0 ? extensions : [".msg", ".cmd"];

        Directory.CreateDirectory(EmulatorDirectory);
        Directory
            .EnumerateFiles(EmulatorDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .ForEach(File.Delete);
        subFolders.Select(sub => Path.Combine(EmulatorDirectory, sub))
            .Where(Directory.Exists)
            .ForEach(DeleteDirectory);
    }

    private void DeleteDirectory(string path)
    {
        var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };
        foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
        {
            info.Attributes = FileAttributes.Normal;
        }
        directory.Delete(true);
    }

    private void BuildEmulator()
    {
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = @$"publish ""{ProjectPath}"" -c Release -o ""{EmulatorDirectory}"" -p:UseAppHost=false";
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

    private void StartEmulator()
    {
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = Path.Combine(EmulatorDirectory, "CashFlowBotEmulator.dll");
        process.StartInfo.WorkingDirectory = EmulatorDirectory;
        process.StartInfo.UseShellExecute = false;
        process.Start();
    }

    public void SendMessage(string message, long? chatId = null)
    {
        var lastReply = chatId.HasValue ? GetReply(chatId.Value) : null;
        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{chatId}";
        var tmpFile = Path.Combine(EmulatorDirectory, $"{fileName}.txt");
        var finalFile = Path.Combine(EmulatorDirectory, $"{fileName}.cmd");

        File.WriteAllText(tmpFile, message);
        File.Move(tmpFile, finalFile);
        WaitForReply(chatId, lastReply);
        Thread.Sleep(100);
    }

    private void WaitForReply(long? chatId, MessageDto lastReply)
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

    public MessageDto GetReply(long chatId, int indexFromEnd = 0)
    {
        var fileName = Path.Combine(EmulatorDirectory, $"{chatId}.msg");
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

    private readonly RetryPolicy _retryPolicy =
        Policy
            .Handle<IOException>()
            .WaitAndRetry(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(200 * retryAttempt));
}
