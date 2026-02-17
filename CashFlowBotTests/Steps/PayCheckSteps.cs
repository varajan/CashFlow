using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class PayCheckSteps(StepsContext context) : BaseSteps(context)
{
    [When("I get a paycheck")]
    public void PayCheck() => User.SendMessage("Paycheck");
}
