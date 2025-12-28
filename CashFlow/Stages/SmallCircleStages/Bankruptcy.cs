using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages;

public class Bankruptcy(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(129, CurrentUser, "You are bankrupt. Game is over.");

    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(41, CurrentUser, "Stop Game"),
        Terms.Get(2, CurrentUser, "History")
    ];

    public override Task HandleMessage(string message)
    {
        if (MessageEquals(message, 41, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, 2, "History"))
        {
            NextStage = New<History>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class BankruptcySellAssets(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
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
}
