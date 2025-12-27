using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages;

public class Bankruptcy(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService)
{
    private IPersonManager PersonManager { get; init; } = personManager;

    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser.Id);
            var cashFlow = Terms.Get(55, CurrentUser, "Cash Flow");
            var cash = Terms.Get(51, CurrentUser, "Cash");
            var bankLoan = Terms.Get(47, CurrentUser, "Bank Loan");

            var message = $"*{Terms.Get(126, CurrentUser, "You're out of money.")}*";
            message += Environment.NewLine + $"{bankLoan}: *{person.Liabilities_OBSOLETE.BankLoan.AsCurrency()}*";
            message += Environment.NewLine + $"{cashFlow}: *{person.CashFlow.AsCurrency()}*";
            message += Environment.NewLine + $"{cash}: *{person.Cash.AsCurrency()}*";

            return message;
        }
    }
    // message
    // buttons
    // handle message
}