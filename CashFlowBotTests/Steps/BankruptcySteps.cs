using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class BankruptcySteps(StepsContext context) : BaseSteps(context)
{
    [When("I sell (.*) asset")]
    public void SellAsset(string title)
    {
        var reply = User.GetReply();
        var button = reply.Message
            .Escape()
            .Split("\n")
            .First(x => x.Contains(title, StringComparison.InvariantCultureIgnoreCase))
            .Split(" ")
            .First();
        User.SendMessage(button);
    }

    [Then("My last message is:")]
    public void CheckLastMessage(string expected)
    {
        var reply = User.GetReply();
        Assert.That(reply.Message, Is.EqualTo(expected));
    }
}
