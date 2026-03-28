using CashFlow.Extensions;
using CashFlowBotSystemTests.Extras;
using MoreLinq;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class BankruptcySteps(StepsContext context) : BaseSteps(context)
{
    private string BankruptcyMessage { get; set; }

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

    [When(@"I sell all (\d+) assets")]
    public void SellAssets(int count) => Enumerable.Range(0, count).ForEach(_ => SellAsset("#1"));

    [Then("My last message is:")]
    public void CheckLastMessageMultiLine(string expected)
    {
        var reply = User.GetReply();
        Assert.That(reply.Message, Is.EqualTo(expected));
    }

    [Then("My last message is: (.*)")]
    public void CheckLastMessage(string expected)
    {
        var reply = User.GetReply();
        Assert.That(reply.Message, Is.EqualTo(expected));
    }

    [When("I pay (.*) to Bank")]
    public void PayMoney(string amount)
    {
        User.SendMessage("Give money");
        User.SendMessage("Bank");
        User.SendMessage(amount);
    }

    [When("I see bankruptcy message")]
    public void RememberBankruptcyMessage() => BankruptcyMessage = User.GetReply().Message;

    [Then("I see my bankruptcy message")]
    public void CheckBankruptcyMessage() => Assert.That(User.GetReply().Message, Is.EqualTo(BankruptcyMessage));
}
