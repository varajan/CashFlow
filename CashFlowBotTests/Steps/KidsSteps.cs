using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class KidsSteps(StepsContext context) : BaseSteps(context)
{
    [When(@"(I|.*) get(|s) (\d+) kid(s|)")]
    public void GetKids(string name, string _, int count, string __)
    {
        for (int i = 0; i < count; i++)
        {
            GetUser(name).SendMessage("Baby");
        }
    }
}
