using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilities(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    private IEnumerable<LiabilityDto> Liabilities => PersonService.Read(CurrentUser).Liabilities.Where(l => l.FullAmount > 0);

    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var cashTerm = Terms.Get("Cash", CurrentUser);
            var monthly = Terms.Get("monthly", CurrentUser);
            var message = "";

            foreach (var liability in Liabilities)
            {
                var name = Terms.Get(liability.Type.AsString(), CurrentUser);
                var fullAmount = liability.FullAmount;
                var cashflow = Math.Abs(liability.Cashflow);

                message += $"{Environment.NewLine}*{name}:* {fullAmount.AsCurrency()} - {cashflow.AsCurrency()} {monthly}";
            }

            return $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}{message}";
        }
    }

    public override IEnumerable<string> Buttons => Liabilities
        .Select(l => Terms.Get(l.Type.AsString(), CurrentUser))
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if ( IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var person = PersonService.Read(CurrentUser);
        var liability = person
            .Liabilities
            .FirstOrDefault(l => !l.Deleted && MessageEquals(message, l.Type.AsString()));

        if (liability is null)
        {
            return;
        }

        var minPaymentAmount = liability.AllowsPartialPayment ? 1_000 : liability.FullAmount;
        if (person.Cash < minPaymentAmount)
        {
            await CurrentUser.Notify(Terms.Get("You don't have {0}, but only {1}", CurrentUser,
                minPaymentAmount.AsCurrency(),
                person.Cash.AsCurrency()));
            return;
        }

        liability.MarkedForReduction = true;
        PersonService.Update(CurrentUser, liability);

        NextStage = liability.AllowsPartialPayment ? New<ReduceLiabilitiesAmount>() : New<ReduceLiabilitiesConfirm>();
    }
}
