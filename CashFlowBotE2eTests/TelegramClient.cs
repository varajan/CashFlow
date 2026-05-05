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

    private string? Config(string what) => what switch
    {
        "api_id" => Environment.GetEnvironmentVariable("API_ID"),
        "api_hash" => Environment.GetEnvironmentVariable("API_HASH"),
        "phone_number" => Environment.GetEnvironmentVariable("PHONE_NUMBER"),
        "device_model" => "Desktop",
        "system_version" => "Windows 11",
        "app_version" => "1.0",
        "lang_code" => "en",
        "session_pathname" => "testing_session.session",
        _ => null
    };

    public static string BotUsername => Environment.GetEnvironmentVariable("BOT_NAME") ?? "varajankoBot";

    //public static string BotUsername => Environment.GetEnvironmentVariable("BOT_NAME")!;

    public void Dispose() => _client?.Dispose();

    public async Task Init()
    {
        await Client.LoginUserIfNeeded();
        var resolveBot = await Client.Contacts_ResolveUsername(BotUsername);
        _botUser = resolveBot.User;
    }

    public void SendMessage(string message)
    {
        Client.SendMessageAsync(_botUser, message).Wait();
        HumanLikeDelay();
    }

    private void HumanLikeDelay() => Thread.Sleep(Random.Next(5_000, 10_000));

    public string? GetLastMessage()
    {
        var history = Client.Messages_GetHistory(_botUser, limit: 1).Result;
        var message = history.Messages?.FirstOrDefault()?.ToString();
        return message;
    }
}
