using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class KidsSteps(StepsContext context) : BaseSteps(context)
{
    [When(@"I get (\d+) kid(s|)")]
    public void GetKids(int count, string _)
    {
        for (int i = 0; i < count; i++)
        {
            User.SendMessage("Baby");
        }
    }
}
