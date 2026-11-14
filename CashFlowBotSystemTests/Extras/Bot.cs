using CashFlow.Extensions;
using MoreLinq;
using System.Diagnostics;

namespace CashFlowBotSystemTests.Extras;

public class Bot(string folder)
{
    private static string Root => Path.GetFullPath("./../../../..");
    private static string ProjectPath => Path.Combine(Root, "CashFlowBotEmulator", "CashFlowBotEmulator.csproj");
    private string EmulatorDirectory => Path.Combine(Root, "emulator", folder.Replace(" ", ""));

    private static readonly Mutex _lock = new(false, "LOCK");

    private Process BotEmulatorProcess;
    private readonly List<string> _consoleOutput = [];
    private readonly object _outputLock = new();

    public void Launch()
    {
        _lock.WaitOne();
        try
        {
            CleanEmulatorDirectory();
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
        CleanEmulatorDirectory();
    }

    private void CleanEmulatorDirectory()
    {
        string[] subFolders = ["Data", "runtimes"];

        Directory.CreateDirectory(EmulatorDirectory);
        Directory.EnumerateFiles(EmulatorDirectory).ForEach(File.Delete);
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
        BotEmulatorProcess = new Process();
        BotEmulatorProcess.StartInfo.FileName = "dotnet";
        BotEmulatorProcess.StartInfo.Arguments = Path.Combine(EmulatorDirectory, "CashFlowBotEmulator.dll");
        BotEmulatorProcess.StartInfo.WorkingDirectory = EmulatorDirectory;
        BotEmulatorProcess.StartInfo.RedirectStandardInput = true;
        BotEmulatorProcess.StartInfo.RedirectStandardOutput = true;
        BotEmulatorProcess.StartInfo.RedirectStandardError = true;
        BotEmulatorProcess.StartInfo.UseShellExecute = false;
        BotEmulatorProcess.StartInfo.CreateNoWindow = true;
        BotEmulatorProcess.StartInfo.StandardInputEncoding = System.Text.Encoding.UTF8;
        BotEmulatorProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

        BotEmulatorProcess.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (_outputLock)
                {
                    _consoleOutput.Add(args.Data);
                }
            }
        };

        BotEmulatorProcess.Start();
        BotEmulatorProcess.BeginOutputReadLine();
    }

    public void SendMessage(string message, long? chatId = null)
    {
        var payload = chatId.HasValue ? $"{chatId} {message}" : message;
        var lastReplyCount = GetTotalRepliesCount(chatId);

        BotEmulatorProcess.StandardInput.WriteLine(payload);
        WaitForReply(chatId, lastReplyCount);
        Thread.Sleep(100);
    }

    private int GetTotalRepliesCount(long? chatId)
    {
        if (!chatId.HasValue) return 0;
        lock (_outputLock)
        {
            return _consoleOutput.Count(line => line.StartsWith($"<- {chatId}"));
        }
    }

    private void WaitForReply(long? chatId, int previousCount)
    {
        if (!chatId.HasValue) return;

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Thread.Sleep(50);
            if (GetTotalRepliesCount(chatId) > previousCount)
            {
                return;
            }
        }

        throw new TimeoutException($"{DateTime.UtcNow} [{chatId}] No reply from bot within the expected time.");
    }

    public MessageDto GetReply(long chatId, int indexFromEnd = 0)
    {
        lock (_outputLock)
        {
            var reptyToUser = $"<- {chatId} ";
            var userLines = _consoleOutput
                .Where(line => line.StartsWith(reptyToUser))
                .ToList();

            if (userLines.Count == 0 || userLines.Count <= indexFromEnd)
            {
                return null;
            }

            var targetLine = userLines[^(indexFromEnd + 1)];
            var reply = targetLine.SubString(reptyToUser).Deserialize<MessageDto>();
            reply.Message = reply.Message?.Trim();

            return reply;
        }
    }

    public IReadOnlyList<string> ConsoleOutput
    {
        get
        {
            lock (_outputLock)
            {
                return _consoleOutput.ToList();
            }
        }
    }
}
