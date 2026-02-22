using CashFlow.Extensions;
using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class GiveMoneySteps(StepsContext context) : BaseSteps(context)
{
    [When("(.*) pays (.*) to (.*)")]
    public void PayMoney(string from, string amount, string to)
    {
        var user = GetUser(from);
        user.SendMessage("Give money");
        user.SendMessage(to);
        user.SendMessage(amount);
    }

    [Then("All users recieve notification:")]
    public void CheckNotification(string notification)
    {
        Assert.Multiple(() =>
        {
            foreach (var user in Context.Users)
            {
                var prevReply = user.GetReply(indexFromEnd: 1).Message;
                var lastReply = user.GetReply(indexFromEnd: 0).Message;
                Assert.That(new[] { prevReply, lastReply }, Does.Contain(notification),
                    $"User {user.Name} did not receive expected notification");
            }
        });
    }

    [Then("Balance by users is:")]
    public void CheckBalance(Table table)
    {
        Assert.Multiple(() =>
        {
            foreach (var row in table.Rows)
            {
                var user = GetUser(row["Name"]);
                var expected = row["Balance"];

                user.SendMessage("Show my Data");
                var reply = user.GetReply();
                var cashLine = reply.Message
                    .Escape()
                    .Split("\n")
                    .First(line => line.Contains("Cash:"));
                var actual = cashLine.Split(" ").Last().Trim();
                Assert.That(actual, Is.EqualTo(expected), $"Wrong balance value for {user.Name}");
            }
        });
    }
}
