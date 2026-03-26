using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class CreditSteps(StepsContext context) : BaseSteps(context)
{
    [Given("(.*) get (.*) as a credit")]
    [When("(.*) get (.*) as a credit")]
    public void GetCredit(string name, string amount)
    {
        var user = GetUser(name);
        user.SendMessage("Show my data");
        user.SendMessage("Get credit");
        user.SendMessage(amount);
    }
}
