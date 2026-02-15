using CashFlow.Data.DTOs;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilities(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    private IEnumerable<LiabilityDto> Liabilities => PersonManager.Read(CurrentUser).Liabilities.Where(l => l.FullAmount > 0);

    public override string Message
    {
        get
        {
            var person = PersonManager.Read(CurrentUser);
            var cashTerm = Terms.Get(51, CurrentUser, "Cash");
            var monthly = Terms.Get(42, CurrentUser, "monthly");
            var message = "";

            foreach (var liability in Liabilities)
            {
                var name = Terms.Get((int) liability.Type, CurrentUser, liability.Type.AsString());
                var fullAmount = liability.FullAmount;
                var cashflow = Math.Abs(liability.Cashflow);

                message += $"{Environment.NewLine}*{name}:* {fullAmount.AsCurrency()} - {cashflow.AsCurrency()} {monthly}";
            }

            return $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}{message}";
        }
    }

    public override IEnumerable<string> Buttons => Liabilities
        .Select(l => Terms.Get((int)l.Type, CurrentUser, l.Type.AsString()))
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if ( IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var person = PersonManager.Read(CurrentUser);
        var liability = person
            .Liabilities
            .FirstOrDefault(l => !l.Deleted && MessageEquals(message, (int)l.Type, l.Type.AsString()));

        if (liability is null)
        {
            return;
        }

        var minPaymentAmount = liability.AllowsPartialPayment ? 1_000 : liability.FullAmount;
        if (person.Cash < minPaymentAmount)
        {
            await CurrentUser.Notify(Terms.Get(23, CurrentUser, "You don't have {0}, but only {1}",
                minPaymentAmount.AsCurrency(),
                person.Cash.AsCurrency()));
            return;
        }

        liability.MarkedForReduction = true;
        PersonManager.Update(CurrentUser, liability);

        NextStage = liability.AllowsPartialPayment ? New<ReduceLiabilitiesAmount>() : New<ReduceLiabilitiesConfirm>();
    }
}
