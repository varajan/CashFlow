using CashFlowBotTests.Extras;
using TechTalk.SpecFlow;

namespace CashFlowBotTests.Steps;

[Binding]
public class ReduceLiabilitiesSteps(StepsContext context) : BaseSteps(context)
{
    [When("I pay off my (.*)")]
    public void PayOff(string liability)
    {
        User.SendMessage("Show my data");
        User.SendMessage("Reduce Liabilities");
        User.SendMessage(liability);
        User.SendMessage("Yes");
    }

    [When("I pay off (.*) of my (Bank Loan)")]
    public void PayOff(string amount, string bankLoan)
    {
        User.SendMessage("Show my data");
        User.SendMessage("Reduce Liabilities");
        User.SendMessage(bankLoan);
        User.SendMessage(amount);
    }
}
