using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class CharitySteps(StepsContext context) : BaseSteps(context)
{
    [When("I donate to a charity")]
    public void Charity()
    {
        User.SendMessage("Show my data");
        User.SendMessage("Charity - Pay 10%");
    }
}
