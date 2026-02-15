using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class StopGameSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;
    private string Profession => _context.Profession;

    [When("I decide to stop the game")]
    public void GoToStopGame()
    {
        User.SendMessage("Show my data");
        User.SendMessage("Stop game");
    }

    [Then("The game is restarted")]
    public void CheckGameIsRestarted()
    {
        var reply = User.GetReply();

        Assert.That(reply.Message, Is.EqualTo("Choose your *profession*"));
    }

    [Then("The game is continued")]
    public void CheckGameIsContinued()
    {
        var reply = User.GetReply();

        Assert.That(reply.Message, Does.StartWith($"*Profession:* {Profession}"));
    }
}
