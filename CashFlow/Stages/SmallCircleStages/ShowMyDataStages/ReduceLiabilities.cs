using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilities(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService)
{
    protected IPersonManager PersonManager { get; } = personManager;

    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser.Id);
            var liabilities = person.Liabilities.Where(l => !l.Deleted);
            var cashTerm = Terms.Get(51, CurrentUser, "Cash");
            var monthly = Terms.Get(42, CurrentUser, "monthly");
            var message = "";

            foreach (var liability in liabilities)
            {
                var name = Terms.Get(-1, CurrentUser, liability.Name);
                var fullAmount = liability.FullAmount;
                var cashflow = liability.Cashflow * -1;

                message += $"{Environment.NewLine}*{name}:* {fullAmount.AsCurrency()} - {cashflow.AsCurrency()} {monthly}";
            }

            return $"*{cashTerm}:* {person.Cash.AsCurrency()}{message}";
        }
    }

    public override IEnumerable<string> Buttons => PersonManager.Read(CurrentUser.Id)
        .Liabilities
        .Select(l => Terms.Get(-1, CurrentUser, l.Name))
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if ( IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var liability = PersonManager.Read(CurrentUser.Id)
            .Liabilities
            .FirstOrDefault(l => !l.Deleted && l.Name.Equals(message, StringComparison.OrdinalIgnoreCase));

        if (liability is null)
        {
            return;
        }

        var minPaymentAmount = liability.AllowsPartialPayment ? 1_000 : liability.FullAmount;
        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < minPaymentAmount)
        {
            await CurrentUser.Notify(Terms.Get(23, CurrentUser, "You don't have {0}, but only {1}",
                minPaymentAmount.AsCurrency(),
                person.Cash.AsCurrency()));
            return;
        }

        liability.MarkedForReduction = true;
        PersonManager.UpdateLiability(CurrentUser.Id, liability);

        NextStage = liability.AllowsPartialPayment ? New<ReduceLiabilitiesAmount>() : New<ReduceLiabilitiesAmount>();
    }
}
