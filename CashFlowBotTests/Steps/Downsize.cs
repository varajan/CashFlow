using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class Downsize(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [When("I lost my job")]
    public void LostJob() => User.SendMessage("Downsize");
}
