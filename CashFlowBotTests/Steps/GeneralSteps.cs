using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class BaseSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [Given(@"I am '(.*)' user")]
    public void SetName(string userName)
    {
        _context.User = new User(userName);
    }

    [Given(@"I play as '(.*)'")]
    public void StartGame(string role)
    {
        User.SendMessage("start");
        User.SendMessage("en");
        User.SendMessage(role);
    }

    [Given(@"I get (.*) in cash")]
    public void GetMoney(string amount)
    {
        User.SendMessage("Show my Data");
        User.SendMessage("Get Money");
        User.SendMessage(amount);
    }

    [Given(@"I get pay check")]
    public void GetPayCheck() => User.SendMessage("Pay Check");

    [Then(@"My Data is following:")]
    public void CheckMyData(string expected)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        Assert.That(reply.Message.Escape(), Is.EqualTo(expected.Escape()));
    }

    [Then(@"My history data is following:")]
    public void CheckHistory(string expected)
    {
        User.SendMessage("History");
        var reply = User.GetReply();
        Assert.That(reply.Message.Escape(), Is.EqualTo(expected.Escape()));
    }

    [When(@"I rollback last action")]
    public void RollbackLastTransaction()
    {
        User.SendMessage("History");
        User.SendMessage("Rollback last action");
        User.SendMessage("Main menu");
    }

    [Then(@"I have (.*) in cash")]
    public void CheckCash(string expectedCash)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var cashLine = reply.Message
            .Escape()
            .Split("\n")
            .First(line => line.Contains("Cash:"));
        var actualCash = cashLine.Split(" ").Last().Trim();
        Assert.That(actualCash, Is.EqualTo(expectedCash));
    }

    [Then(@"My assets are:")]
    public void CheckAssets(string assets)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var expected = assets.Escape().Split("\n").ToList();
        var actual = reply.Message.SubString("*Assets:*", "*Expenses:*")
            .Escape()
            .Split("\n")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
    }
}
