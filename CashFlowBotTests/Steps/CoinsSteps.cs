using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class CoinsSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [Given(@"I buy (\d+) (Peso|Krugerand)(s|) with price (.*) each")]
    [When(@"I buy (\d+) (Peso|Krugerand)(s|) with price (.*) each")]
    public void BuyCoins(string count, string name, string _, string price)
    {
        User.SendMessage("Small Opportunity");
        User.SendMessage("Buy Coins");
        User.SendMessage(name);
        User.SendMessage(count);
        User.SendMessage(price);
    }

    [Given(@"I sell (Peso|Krugerand)s for (.*) each")]
    [When(@"I sell (Peso|Krugerand)s for (.*) each")]
    public void SellCoins(string name, string price)
    {
        User.SendMessage("Market");
        User.SendMessage("Sell Coins");
        User.SendMessage(name);
        User.SendMessage(price);
    }
}
