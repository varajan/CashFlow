using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class BusinessSteps(StepsContext context) : BaseSteps(context)
{
    [Given("(I|.*) buy(|s) businesses:")]
    [When ("(I|.*) buy(|s) businesses:")]
    public void BuyRealEstate(string name, string _, Table table)
    {
        var user = GetUser(name);

        foreach (var row in table.Rows)
        {
            var title = row["Title"];
            var price = row["Price"];
            var firstPayment = row["First Payment"];
            var cashflow = row["Monthly Cashflow"];

            user.SendMessage("Big Opportunity");
            user.SendMessage("Buy Business");
            user.SendMessage(title);
            user.SendMessage(price);
            user.SendMessage(firstPayment);
            user.SendMessage(cashflow);
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
