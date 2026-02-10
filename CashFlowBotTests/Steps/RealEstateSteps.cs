using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class RealEstateSteps(StepsContext context)
{
    private readonly StepsContext _context = context;
    private User User => _context.User;

    [When("I buy real estate:")]
    public void BuyRealEstate(Table table)
    {
        foreach (var row in table.Rows)
        {
            var type = row["Opportunity"];
            var title = row["Title"];
            var price = row["Price"];
            var firstPayment = row["First Payment"];
            var cashflow = row["Monthly Cashflow"];

            User.SendMessage($"{type} Opportunity");
            User.SendMessage("Buy Real Estate");
            User.SendMessage(title);
            User.SendMessage(price);
            User.SendMessage(firstPayment);
            User.SendMessage(cashflow);
        }
    }
}
