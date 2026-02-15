using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class KidsSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [When(@"I get (\d+) kid(s|)")]
    public void GetKids(int count, string _)
    {
        for (int i = 0; i < count; i++)
        {
            User.SendMessage("Baby");
        }
    }
}
