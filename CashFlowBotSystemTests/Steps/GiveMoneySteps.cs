using CashFlow.Extensions;
using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class GiveMoneySteps(StepsContext context) : BaseSteps(context)
{
    [Scope(Feature = "GiveMoney")]
    [When("(.*) pays (.*) to (.*)")]
    public void PayMoney(string from, string amount, string to)
    {
        var user = GetUser(from);
        user.SendMessage("Give money");
        user.SendMessage(to);
        user.SendMessage(amount);
    }

    [Then("All users recieve notification: (.*)")]
    public void CheckNotification(string notification) => CheckNotification(Context.Users, notification);

    [Then("All users, except (.*) recieve notification: (.*)")]
    public void CheckNotification(string name, string notification)
    {
        var user = GetUser(name);
        var users = Context.Users.Where(u => u.Name != name).ToList();
        var prevReply = user.GetReply(indexFromEnd: 1).Message;
        var lastReply = user.GetReply(indexFromEnd: 0).Message;

        CheckNotification(users, notification);
        Assert.That(new[] { prevReply, lastReply }, Does.Not.Contain(notification),
            $"{name} received unexpected notification");
    }

    private static void CheckNotification(List<User> users, string notification)
    {
        Assert.Multiple(() =>
        {
            foreach (var user in users)
            {
                var prevReply = user.GetReply(indexFromEnd: 1).Message;
                var lastReply = user.GetReply(indexFromEnd: 0).Message;
                Assert.That(new[] { prevReply, lastReply }, Does.Contain(notification),
                    $"{user.Name} did not receive expected notification");
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
