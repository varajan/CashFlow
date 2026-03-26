using CashFlowBotTests.Extras;
using MoreLinq;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class Downsize(StepsContext context) : BaseSteps(context)
{
    [When("I lost my job")]
    public void LostJob() => User.SendMessage("Downsize");

    [When(@"I lost my job (\d+) times")]
    public void LostJobTimes(int times) => Enumerable.Range(0, times).ForEach(_ => LostJob());
}
