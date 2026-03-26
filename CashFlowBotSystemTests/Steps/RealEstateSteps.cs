using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class RealEstateSteps(StepsContext context) : BaseSteps(context)
{
    [When("(.*) buy(s|) real estate:")]
    public void BuyRealEstate(string name, string _, Table table)
    {
        foreach (var row in table.Rows)
        {
            var user = GetUser(name);
            var type = row["Opportunity"];
            var title = row["Title"];
            var price = row["Price"];
            var firstPayment = row["First Payment"];
            var cashflow = row["Monthly Cashflow"];

            user.SendMessage($"{type} Opportunity");
            user.SendMessage("Buy Real Estate");
            user.SendMessage(title);
            user.SendMessage(price);
            user.SendMessage(firstPayment);
            user.SendMessage(cashflow);
        }
    }

    [Scope(Feature = "RealEstate")]
    [When(@"I sell (.*) for (\S*)( each|)")]
    public void SellRealEstate(string title, string price, string _)
    {
        User.SendMessage("Market");
        User.SendMessage("Sell Real Estate");
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
