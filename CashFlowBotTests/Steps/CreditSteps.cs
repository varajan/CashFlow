using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class CreditSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [Given("I get (.*) as a credit")]
    [When("I get (.*) as a credit")]
    public void GetCredit(string amount)
    {
        User.SendMessage("Show my data");
        User.SendMessage("Get credit");
        User.SendMessage(amount);
    }
}
