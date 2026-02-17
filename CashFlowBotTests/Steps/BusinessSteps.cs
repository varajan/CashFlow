using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class BusinessSteps(StepsContext context) : BaseSteps(context)
{
    [Given("I buy businesses:")]
    [When("I buy businesses:")]
    public void BuyRealEstate(Table table)
    {
        foreach (var row in table.Rows)
        {
            var title = row["Title"];
            var price = row["Price"];
            var firstPayment = row["First Payment"];
            var cashflow = row["Monthly Cashflow"];

            User.SendMessage("Big Opportunity");
            User.SendMessage("Buy Business");
            User.SendMessage(title);
            User.SendMessage(price);
            User.SendMessage(firstPayment);
            User.SendMessage(cashflow);
        }
    }

    [Scope(Feature = "Business")]
    [When(@"I sell (.*) for (.*)")]
    public void SellRealEstate(string title, string price)
    {
        User.SendMessage("Market");
        User.SendMessage("Sell Business");
        var message = User.GetReply().Message;
        var button = message
            .Escape()
            .Split("\n")
            .First(x => x.Contains(title, StringComparison.InvariantCultureIgnoreCase))
            .Split(" ")
            .First()
            .SubString("*", "*");
        User.SendMessage(button);
        User.SendMessage(price);
    }
}
