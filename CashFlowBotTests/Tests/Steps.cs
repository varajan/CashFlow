using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Tests;

[Binding]
public class Steps
{
    private User user;

    [Given(@"I am '(.*)' user")]
    public void SetName(string userName) => user = new User(userName);

    [Given(@"I play as '(.*)'")]
    public void StartGame(string role)
    {
        user.SendMessage("start");
        user.SendMessage("en");
        user.SendMessage(role);
    }

    [Given(@"I buy (\d+) shares of '(.*)' stock with price '(.*)' each")]
    [When(@"I buy (\d+) shares of '(.*)' stock with price '(.*)' each")]
    public void BuyStocks(int count, string name, string price)
    {
        user.SendMessage("Small Opportunity");
        user.SendMessage("Buy stocks");
        user.SendMessage(name);
        user.SendMessage(price);
        user.SendMessage(count.ToString());

        var reply = user.GetReply();
        if (reply.Buttons.First() == "Get Credit")
        {
            user.SendMessage("Get Credit");
        }
    }

    [When(@"I sell '(.*)' stock with price '(.*)' each")]
    public void SellStocks(string name, string price)
    {
        user.SendMessage("Small Opportunity");
        user.SendMessage("Sell stocks");
        user.SendMessage(name);
        user.SendMessage(price);
    }

    [When(@"I (multiply|divide) '(.*)' stocks")]
    public void MultiplyStocks(string action, string name)
    {
        user.SendMessage("Small Opportunity");
        user.SendMessage(action == "multiply" ? "Stocks x2" : "Stocks \u00F72");
        user.SendMessage(name);
    }

    [Then(@"My Data is following:")]
    public void CheckMyData(string expected)
    {
        user.SendMessage("Show my Data");
        var reply = user.GetReply();
        Assert.That(reply.Message.Escape(), Is.EqualTo(expected.Escape()));
    }
}
