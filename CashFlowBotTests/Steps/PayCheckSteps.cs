using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class PayCheckSteps(StepsContext context) : BaseSteps(context)
{
    [When("(I|.*) get(|s) a paycheck")]
    public void PayCheck(string name, string _) => GetUser(name).SendMessage("Paycheck");
}
