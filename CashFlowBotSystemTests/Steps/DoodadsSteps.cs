using CashFlowBotSystemTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotSystemTests.Steps;

[Binding]
public class DoodadsSteps(StepsContext context) : BaseSteps(context)
{
    [When("I'm buying (.*) on impulse")]
    public void GoToDoodads(string _) => User.SendMessage("Doodads");

    [When("I pay (.*) with (cash|credit card)")]
    public void Pay(string amount, string way)
    {
        User.SendMessage($"Pay with {way}");
        User.SendMessage(amount);
    }

    [Given("(I|.*) buy(|s) a boat")]
    [When ("(I|.*) buy(|s) a boat")]
    public void BuyBoat(string name, string _)
    {
        var user = GetUser(name);

        user.SendMessage("Doodads");
        user.SendMessage("Buy a boat");
    }
}
