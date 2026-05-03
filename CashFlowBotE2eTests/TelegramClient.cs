using TL;
using WTelegram;

namespace CashFlowBotE2eTests;

public class TelegramClient : IDisposable
{
    private Client _client;
    private Client Client => _client ??= new Client(Config);

    private User botUser;

    private string Config(string what) => what switch
    {
        "api_id" => Environment.GetEnvironmentVariable("API_ID"),
        "api_hash" => Environment.GetEnvironmentVariable("API_HASH"),
        "phone_number" => Environment.GetEnvironmentVariable("PHONE_NUMBER"),
        "verification_code" => Environment.GetEnvironmentVariable("VERIFICATION_CODE"),
        "session_pathname" => "testing_session.session",
        //"password" => "qwerty123",
        _ => null
    };

    public static string BotUsername => "varajankoBot";

    public void Dispose() => _client?.Dispose();

    public async Task Init()
    {
        await Client.LoginUserIfNeeded();
        var resolveBot = await Client.Contacts_ResolveUsername(BotUsername);
        botUser = resolveBot.User;
    }

    public async Task SendMessage(string message)
    {
        await Client.SendMessageAsync(botUser, message);
        await Task.Delay(1_000);
    }

    public async Task<string> GetLastMessage()
    {
        var history = await Client.Messages_GetHistory(botUser, limit: 1);
        var message = history.Messages?.FirstOrDefault()?.ToString();
        return message;
    }
}