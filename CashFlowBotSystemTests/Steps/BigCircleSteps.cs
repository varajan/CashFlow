using CashFlow.Extensions;
using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class BigCircleSteps(StepsContext context) : BaseSteps(context)
{
    [Scope(Feature = "BigCircle")]
    [When("(.*) pay(s|) (.*)")]
    public void Pay(string name, string _, string amount)
    {
        var user = GetUser(name);
        user.SendMessage("Give Money");
        user.SendMessage(amount);
    }

    [When("(.*) loses money because of (.*)")]
    public void LoseMoney(string name, string reason) => GetUser(name).SendMessage(reason);

    [Then(@"(My|.*) last history record is: '(.*)'")]
    public void CheckHistory(string name, string expected)
    {
        var user = GetUser(name);
        user.SendMessage("Main menu");
        user.SendMessage("History");
        var actual = user.GetReply().Message.Split(Environment.NewLine).Last();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Given("(.*) buys big businesses:")]
    [When ("(.*) buys big businesses:")]
    public void BuyRealEstate(string name, Table table)
    {
        var user = GetUser(name);

        foreach (var row in table.Rows)
        {
            var title = row["Title"];
            var price = row["Price"];
            var cashflow = row["Cashflow"];

            user.SendMessage("Buy Business");
            user.SendMessage(title);
            user.SendMessage(price);
            user.SendMessage(cashflow);
        }
    }

    [Then("(.*)' details are following:")]
    public void CheckDetails(string name, string expected)
    {
        var user = GetUser(name);
        var actual = user.GetReply().Message;
        Assert.That(actual.Escape(), Is.EqualTo(expected.Escape()));
    }

    [Then("(.*) recieved notification: '(.*)'")]
    public void CheckNotification(string name, string expected)
    {
        var user = GetUser(name);
        var actual = user.GetReply().Message;
        Assert.That(actual, Is.EqualTo(expected));
    }
}
