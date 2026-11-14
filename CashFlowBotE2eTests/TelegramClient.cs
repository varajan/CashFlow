using TL;
using WTelegram;

namespace CashFlowBotE2eTests;

public class TelegramClient : IDisposable
{
    private Client? _client;
    private Client Client => _client ??= new Client(Config);

    private Random? _random;
    private Random Random => _random ??= new Random();

    private User? _botUser;

    /// Create a "TESTING_SESSION" secret content for GIT with command:
    /// powershell -Command "[Convert]::ToBase64String([System.IO.File]::ReadAllBytes('testing_session.session')) | Out-File -Encoding ascii -NoNewline base64_session.txt"

    private string? Config(string what) => what switch
    {
        "api_id" => Environment.GetEnvironmentVariable("API_ID") ?? "",
        "api_hash" => Environment.GetEnvironmentVariable("API_HASH") ?? "",
        "phone_number" => Environment.GetEnvironmentVariable("PHONE_NUMBER") ?? "",
        "verification_code" => "", // put breakpoint here and enter the code from Telegram
        "device_model" => "Desktop",
        "system_version" => "Windows 11",
        "app_version" => "1.0",
        "lang_code" => "en",
        "session_pathname" => "testing_session.session",
        _ => null
    };

    public static string? BotUsername => Environment.GetEnvironmentVariable("BOT_NAME")?.Replace("@", "");

    public void Dispose() => _client?.Dispose();

    public async Task Init()
    {
        await Client.LoginUserIfNeeded();
        var resolveBot = await Client.Contacts_ResolveUsername(BotUsername);
        _botUser = resolveBot.User;
    }

    public async Task SendMessage(string message)
    {
        await Client.SendMessageAsync(_botUser, message);
        await HumanLikeDelay();
    }

    private async Task HumanLikeDelay() => await Task.Delay(Random.Next(5_000, 10_000));

    public async Task<string?> GetLastMessage()
    {
        var history = await Client.Messages_GetHistory(_botUser, limit: 1);
        var message = history.Messages?.FirstOrDefault()?.ToString();
        return message;
    }
}
