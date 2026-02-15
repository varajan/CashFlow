using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class DoodadsSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [When("I'm buying (.*) on impulse")]
    public void GoToDoodads(string _) => User.SendMessage("Doodads");

    [When("I pay (.*) with (cash|credit card)")]
    public void Pay(string amount, string way)
    {
        User.SendMessage($"Pay with {way}");
        User.SendMessage(amount);
    }

    [Given("I buy a boat")]
    [When("I buy a boat")]
    public void BuyBoat()
    {
        User.SendMessage("Doodads");
        User.SendMessage("Buy a boat");
    }
}
