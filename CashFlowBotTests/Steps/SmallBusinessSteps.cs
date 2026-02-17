using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class SmallBusinessSteps(StepsContext context) : BaseSteps(context)
{
    [Given(@"I start the (Auto Tools|Computer Programs) company with (.*)")]
    [When(@"I start the (Auto Tools|Computer Programs) company with (.*)")]
    public void StartCompany(string name, string price)
    {
        User.SendMessage("Small Opportunity");
        User.SendMessage("Start a company");
        User.SendMessage(name);
        User.SendMessage(price);
    }

    [When(@"I sell (.*) small business for (.*)")]
    public void SellSmallBusiness(string name, string price)
    {
        User.SendMessage("Market");
        User.SendMessage("Sell business");

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

    [Given(@"I increase the cash flow of my small business by (.*)")]
    [When(@"I increase the cash flow of my small business by (.*)")]
    public void IncreaseCashFlow(string amount)
    {
        User.SendMessage("Market");
        User.SendMessage("Increase cash flow");
        User.SendMessage(amount);
    }
}
