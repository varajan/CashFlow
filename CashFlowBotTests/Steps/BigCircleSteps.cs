using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class BigCircleSteps(StepsContext context) : BaseSteps(context)
{
    [When("(.*) pays (.*)")]
    public void Pay(string name, string amount)
    {
        var user = GetUser(name);
        user.SendMessage("Give Money");
        user.SendMessage(amount);
    }

    [When("(.*) loses money because of (.*)")]
    public void LoseMoney(string name, string reason) => GetUser(name).SendMessage(reason);

    [Then(@"(My|.*) last history record is: (.*)")]
    public void CheckHistory(string name, string expected)
    {
        var user = GetUser(name);
        user.SendMessage("Main menu");
        user.SendMessage("History");
        var actual = user.GetReply().Message.Split(Environment.NewLine).Last();

        Assert.That(actual, Is.EqualTo(expected));
    }

}
