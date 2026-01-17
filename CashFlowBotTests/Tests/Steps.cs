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

    [Given(@"I get pay check")]
    public void GetPayCheck() => user.SendMessage("Pay Check");

    [Given(@"I get (.*) in cash")]
    public void GetMoney(string amount)
    {
        user.SendMessage("Show my Data");
        user.SendMessage("Get Money");
        user.SendMessage(amount);
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
    }

    [Given(@"I get credit")]
    [When (@"I get credit")]
    public void GetCredit()
    {
        var reply = user.GetReply();
        if (reply.Buttons.First() == "Get Credit")
        {
            user.SendMessage("Get Credit");
            return;
        }

        Assert.Fail("No credit is suggested");
    }

    [Given(@"I sell '(.*)' stock with price '(.*)' each")]
    [When(@"I sell '(.*)' stock with price '(.*)' each")]
    public void SellStocks(string name, string price)
    {
        user.SendMessage("Small Opportunity");
        user.SendMessage("Sell stocks");
        user.SendMessage(name);
        user.SendMessage(price);
    }

    [Given(@"I (multiply|divide) '(.*)' stocks")]
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

    [Then(@"My history data is following:")]
    public void CheckHistory(string expected)
    {
        user.SendMessage("History");
        var reply = user.GetReply();
        Assert.That(reply.Message.Escape(), Is.EqualTo(expected.Escape()));
    }

    [When(@"I rollback last action")]
    public void RollbackLastTransaction()
    {
        user.SendMessage("History");
        user.SendMessage("Rollback last action");
        user.SendMessage("Main menu");
    }

    [Then(@"I have (.*) in cash")]
    public void CheckCash(string expectedCash)
    {
        user.SendMessage("Show my Data");
        var reply = user.GetReply();
        var cashLine = reply.Message
            .Escape()
            .Split("\n")
            .First(line => line.Contains("Cash:"));
        var actualCash = cashLine.Split(" ").Last().Trim();
        Assert.That(actualCash, Is.EqualTo(expectedCash));
    }

    [Then(@"My assets are:")]
    public void CheckAssets(Table assets)
    {
        var expected = assets.Rows
            .Select(row =>
            {
                var title = row["Title"];
                var qtty = int.Parse(row["Quantity"]);
                var price = row["Price"];

                return $"• *{title}* - {qtty} @ {price}";
            })
            .ToList();

        user.SendMessage("Show my Data");
        var reply = user.GetReply();
        var actual = reply.Message.SubString("*Assets:*", "*Expenses:*")
            .Escape()
            .Split("\n")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
    }
}
