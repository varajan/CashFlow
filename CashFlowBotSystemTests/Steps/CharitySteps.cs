using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

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
