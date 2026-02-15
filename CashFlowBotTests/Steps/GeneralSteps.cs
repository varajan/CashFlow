using CashFlow.Data.Users.UserData.PersonData;
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
    [When(@"I get (.*) in cash")]
    public void GetMoney(string amount)
    {
        User.SendMessage("Show my Data");
        User.SendMessage("Get Money");
        User.SendMessage(amount);
    }

    [Given(@"I get Paycheck")]
    public void GetPayCheck() => User.SendMessage("Paycheck");

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
        User.SendMessage("Main menu");
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

    [When(@"I rollback last (\d+) actions")]
    public void RollbackLastTransaction(int n)
    {
        User.SendMessage("History");
        for (var i = 0; i < n; i++) User.SendMessage("Rollback last action");
        User.SendMessage("Main menu");
    }

    [Given(@"I get credit")]
    [When(@"I get credit")]
    public void GetCredit()
    {
        var reply = User.GetReply();
        if (reply.Buttons.First() == "Get Credit")
        {
            User.SendMessage("Get Credit");
            return;
        }

        Assert.Fail("No credit is suggested");
    }

    [When(@"The cashflow is (.*)")]
    public void CheckCashflow(string cashflow)
    {
        var reply = User.GetReply();
        if (reply.Message == "What is the cash flow?")
        {
            User.SendMessage(cashflow);
            return;
        }

        Assert.Fail("No cash flow message");
    }

    [Then(@"I have (.*) in cash")]
    public void CheckCash(string expected)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var cashLine = reply.Message
            .Escape()
            .Split("\n")
            .First(line => line.Contains("Cash:"));
        var actual = cashLine.Split(" ").Last().Trim();
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Then(@"My passive income is (.*)")]
    public void CheckCashFlow(string expected)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var cashLine = reply.Message
            .Escape()
            .Split("\n")
            .First(line => line.Contains("Income:"));
        var actual = cashLine.Split(" ").Last().Trim();
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Then(@"My expenses are (.*)")]
    public void CheckExpenses(string expected)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var cashLine = reply.Message
            .Escape()
            .Split("\n")
            .First(line => line.Contains("Expenses:"));
        var actual = cashLine.Split(" ").Last().Trim();
        Assert.That(actual, Is.EqualTo(expected));
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

    [Then(@"I have no assets")]
    public void CheckNoAssets()
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        string[] expected = [];
        var actual = reply.Message.SubString("*Assets:*", "*Expenses:*")
            .Escape()
            .Split("\n")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Then("My Expenses are:")]
    public void CheckAllExpenses(string expenses)
    {
        User.SendMessage("Show my Data");
        var reply = User.GetReply();
        var expected = expenses.Escape().Split("\n").ToList();
        var actual = reply.Message
            .SubString("*Expenses:*")
            .SubString("*Expenses:*")
            .Escape()
            .Split("\n")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
    }
}
