namespace CashFlowBotE2eTests;

public class E2ETests
{
    private TelegramClient _client;

    [SetUp]
    public void Setup()
    {
        _client = new TelegramClient();
        _client.Init().Wait();
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    [Test]
    public async Task BasicFlow()
    {
        var messagesAndResponses = new Dictionary<string, string>
        {
            ["start"] = "Language/Мова",
            ["EN"] = "Choose your profession",
            ["Nurse"] = "Profession: Nurse",
            ["Big Opportunity"] = "What do you want?",
            ["Buy Business"] = "Title:",
            ["Company"] = "What is the price?",
            ["100 000"] = "What is the first payment?",
            ["1000"] = "What is the cash flow?",
            ["2000"] = "Profession: Nurse",
            ["Go to Big Circle"] = "Cash: $200,600",
            ["Buy my dream"] = "What is the price?",
            ["200 000"] = "You are the winner!",
            //["Stop Game"] = "?",
            //["Yes"] = "Choose your profession",
        };

        foreach (var (message, expectedResponse) in messagesAndResponses)
        {
            await _client.SendMessage(message);
            var response = await _client.GetLastMessage();
            Assert.That(response, Does.Contain(expectedResponse), $"Failed at message: <{message}>");
        }
    }
}
