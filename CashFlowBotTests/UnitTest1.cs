namespace CashFlowBotTests;

public class Tests
{
    private Bot Bot { get; }

    public Tests() => Bot = new Bot();

    [Test]
    public void SmallOpportunity_Buy_and_Sell_Stocks()
    {
        // Arrange
        var user = new User("Michael Scott", Bot);
        var afterBuyStocks = @"*Profession:* Engineer
*Cash:* $590
*Salary:* $4,900
*Income:* $0
*Expenses:* $3,410
*Cash Flow*: $1,490

*Assets:*
• *OK4U* - 1000 @ $1
• *ON2U* - 500 @ $5

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
        user.SendMessage("start");
        user.SendMessage("en");
        user.SendMessage("Engineer");

        user.SendMessage("Small Opportunity");
        user.SendMessage("Buy stocks");
        user.SendMessage("OK4U");
        user.SendMessage("$1");
        user.SendMessage("1000");


        user.SendMessage("Small Opportunity");
        user.SendMessage("Buy stocks");
        user.SendMessage("ON2U");
        user.SendMessage("$5");
        user.SendMessage("500");
        // You don't have $2,500, but only $1,090
        user.SendMessage("Get credit");

        // Assert
        user.SendMessage("Show my Data");
        var reply = user.GetReply();
        Assert.That(reply.Message, Is.EqualTo(afterBuyStocks));
    }
}