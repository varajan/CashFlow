using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class FriendsSteps(StepsContext context) : BaseSteps(context)
{
    [Given("Few players:")]
    public void AddPlayers(Table table)
    {
        foreach (var row in table.Rows)
        {
            var name = row["Name"];
            var role = row["Profession"];

            var user = new User(name);
            user.SendMessage(name);
            user.StopCurrentGame();
            user.SendMessage("en");
            user.SendMessage(role);

            Context.Users.Add(user);
        }
    }

    [Then("(.*) can see friends:")]
    public void VerifyReply(string name, string text)
    {
        var reply = GetUser(name).GetReply();

        Assert.That(reply.Message, Is.EqualTo(text));
    }

    [Then(@"(.*) can see details:")]
    public void CheckUserData(string name, string text)
    {
        var reply = GetUser(name).GetReply(indexFromEnd: 2);

        Assert.That(reply.Message, Is.EqualTo(text));
    }

    [Then(@"(.*) can see history details:")]
    public void CheckUserHistory(string name, string text)
    {
        var reply = GetUser(name).GetReply(indexFromEnd: 1);
        Assert.That(reply.Message, Is.EqualTo(text));
    }
}
