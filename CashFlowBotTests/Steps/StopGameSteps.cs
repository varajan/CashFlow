using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class StopGameSteps(StepsContext context) : BaseSteps(context)
{
    [When("(I|.*) decide(|s) to stop the game")]
    public void GoToStopGame(string name, string _)
    {
        var user = GetUser(name);

        user.SendMessage("Show my data");
        user.SendMessage("Stop game");
    }

    [Then("The game is restarted for (me|.*)")]
    public void CheckGameIsRestarted(string name)
    {
        var user = GetUser(name);
        var reply = user.GetReply();

        Assert.That(reply.Message, Is.EqualTo("Choose your *profession*"));
    }

    [Then("The game is continued for (me|.*)")]
    public void CheckGameIsContinued(string name)
    {
        var user = GetUser(name);
        var reply = user.GetReply();

        Assert.That(reply.Message, Does.StartWith($"*Profession:* {user.Profession}"));
    }
}
