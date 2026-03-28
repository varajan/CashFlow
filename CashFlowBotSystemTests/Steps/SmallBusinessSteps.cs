using CashFlow.Extensions;
using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class SmallBusinessSteps(StepsContext context) : BaseSteps(context)
{
    [Given(@"(I|.*) start(|s) the (Auto Tools|Computer Programs) company with (.*)")]
    [When (@"(I|.*) start(|s) the (Auto Tools|Computer Programs) company with (.*)")]
    public void StartCompany(string name, string _, string title, string price)
    {
        var user = GetUser(name);

        user.SendMessage("Small Opportunity");
        user.SendMessage("Start a company");
        user.SendMessage(title);
        user.SendMessage(price);
    }

    [When(@"(I|.*) sell(|s) (.*) small business for (.*)")]
    public void SellSmallBusiness(string name, string _, string title, string price)
    {
        var user = GetUser(name);
        user.SendMessage("Market");
        user.SendMessage("Sell business");

        var message = user.GetReply().Message;
        var button = message
            .Escape()
            .Split("\n")
            .First(x => x.Contains(title))
            .Split(" ")
            .First()
            .SubString("*", "*");

        user.SendMessage(button);
        user.SendMessage(price);
    }

    [Given(@"(I|.*) increase(|s) the cash flow of my small business by (.*)")]
    [When (@"(I|.*) increase(|s) the cash flow of my small business by (.*)")]
    public void IncreaseCashFlow(string name, string _, string amount)
    {
        var user = GetUser(name);

        user.SendMessage("Market");
        user.SendMessage("Increase cash flow");
        user.SendMessage(amount);
    }
}
