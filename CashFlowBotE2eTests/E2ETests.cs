namespace CashFlowBotE2eTests;

public class E2ETests
{
    private TelegramClient _client;

    [SetUp]
    public async Task Setup()
    {
        _client = new TelegramClient();
        await _client.Init();
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    [Test]
    public async Task SmokeTest()
    {
        var messagesAndResponses = new Dictionary<string, string>
        {
            ["start"] = "Language/Мова",
            ["EN"] = "Choose your profession",
            ["Nurse"] = "Profession: Nurse",
        };

        foreach (var (message, expectedResponse) in messagesAndResponses)
        {
            await _client.SendMessage(message);
            var response = await _client.GetLastMessage();

            Assert.That(response, Does.Contain(expectedResponse), $"Failed at message: <{message}>");
        }
    }
}
