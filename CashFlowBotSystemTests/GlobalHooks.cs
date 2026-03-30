using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests;

[Binding]
public class GlobalHooks
{

    [BeforeTestRun]
    public static void BeforeTestRun() => Bot.Launch();

    [AfterTestRun]
    public static void AfterTestRun() => Bot.Close();
}
