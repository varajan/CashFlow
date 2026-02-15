using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class PayCheckSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [When("I get a paycheck")]
    public void PayCheck() => User.SendMessage("Paycheck");
}
