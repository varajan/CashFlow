using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class StockSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [Given(@"I buy (\d+) shares of '(.*)' stock with price (.*) each")]
    [When(@"I buy (\d+) shares of '(.*)' stock with price (.*) each")]
    public void BuyStocks(int count, string name, string price)
    {
        User.SendMessage("Small Opportunity");
        User.SendMessage("Buy stocks");
        User.SendMessage(name);
        User.SendMessage(price);
        User.SendMessage(count.ToString());
    }

    [Given(@"I get credit")]
    [When (@"I get credit")]
    public void GetCredit()
    {
        var reply = User.GetReply();
        if (reply.Buttons.First() == "Get Credit")
        {
            User.SendMessage("Get Credit");
            return;
        }

        Assert.Fail("No credit is suggested");
    }

    [Given(@"I sell '(.*)' stock with price (.*) each")]
    [When(@"I sell '(.*)' stock with price (.*) each")]
    public void SellStocks(string name, string price)
    {
        User.SendMessage("Small Opportunity");
        User.SendMessage("Sell stocks");
        User.SendMessage(name);
        User.SendMessage(price);
    }

    [Given(@"I (multiply|divide) '(.*)' stocks")]
    [When(@"I (multiply|divide) '(.*)' stocks")]
    public void MultiplyStocks(string action, string name)
    {
        User.SendMessage("Small Opportunity");
        User.SendMessage(action == "multiply" ? "Stocks x2" : "Stocks \u00F72");
        User.SendMessage(name);
    }
}
