using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class LandSteps(StepsContext context) : BaseSteps(context)
{
    [Given(@"I buy (.*) of land with price (.*) as (Small|Big) opprotunity")]
    [When(@"I buy (.*) of land with price (.*) as (Small|Big) opprotunity")]
    public void BuyLand(string name, string price, string type)
    {
        User.SendMessage($"{type} Opportunity");
        User.SendMessage("Buy Land");
        User.SendMessage(name);
        User.SendMessage(price);
    }

    [When(@"I sell (.*) of land for (.*)")]
    public void SellLand(string name, string price)
    {
        User.SendMessage("Market");
        User.SendMessage("Sell Land");

        var message = User.GetReply().Message;
        var button = message
            .Escape()
            .Split("\n")
            .First(x => x.Contains(name))
            .Split(" ")
            .First()
            .SubString("*", "*");

        User.SendMessage(button);
        User.SendMessage(price);
    }
}
