namespace CashFlowBotTests;

public class Tests
{
    [OneTimeSetUp]
    public void Setup() => Bot.Start();

    [OneTimeTearDown]
    public void TearDown() => Bot.Stop();

    [Test]
    public void SmallOpportunity_Buy_and_Sell_Stocks()
    {
        // Arrange
        var userId = 1;
        var afterBuyStocks = @"*Profession:* Engineer
*Cash:* $590
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,410
*Cash Flow:* $1,490

*Assets:*
• OK4U - 1000 @ $1
• ON2U - 500 @ $5
*Expenses:*
*Taxes:* $1,050
*Mortgage/Rent Pay:* $700
*School Loan:* $60
*Car Loan:* $140
*Credit Card:* $120
*Small Credit:* $50
*Bank Loan:* $200
*Other Payments:* $1,090";

        // Act
        Bot.SendMessage("start", userId);
        Bot.SendMessage("en", userId);
        Bot.SendMessage("Engineer", userId);

        Bot.SendMessage("Small Opportunity", userId);
        Bot.SendMessage("Buy stocks", userId);
        Bot.SendMessage("OK4U", userId);
        Bot.SendMessage("$1", userId);
        Bot.SendMessage("1000", userId);


        Bot.SendMessage("Small Opportunity", userId);
        Bot.SendMessage("Buy stocks", userId);
        Bot.SendMessage("ON2U", userId);
        Bot.SendMessage("$5", userId);
        Bot.SendMessage("500", userId);
        // You don't have $2,500, but only $1,090
        Bot.SendMessage("Get credit", userId);

        // Assert
        var reply = Bot.GetReply(userId);
        Assert.That(reply.Message, Is.EqualTo(afterBuyStocks));
    }
}