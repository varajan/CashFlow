using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

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
            user.SendMessage("start");
            user.SendMessage("en");
            user.SendMessage(role);

            Context.Users.Add(user);
        }
    }
}
