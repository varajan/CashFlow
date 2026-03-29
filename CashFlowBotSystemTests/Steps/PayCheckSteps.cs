using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class PayCheckSteps(StepsContext context) : BaseSteps(context)
{
    [When("(I|.*) get(|s) a paycheck")]
    public void PayCheck(string name, string _)
    {
        GetUser(name).SendMessage("Cancel");
        GetUser(name).SendMessage("Paycheck");
    }
}
