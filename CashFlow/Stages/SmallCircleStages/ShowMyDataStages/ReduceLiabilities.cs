using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilities(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, userService, personManager, userRepository)
{
    private IEnumerable<LiabilityDto> Liabilities => PersonService.Read(CurrentUser).Liabilities.Where(l => l.FullAmount > 0);

    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var cashTerm = TranslationService.Get(Terms.Cash, CurrentUser);
            var monthly = TranslationService.Get(Terms.Monthly, CurrentUser);
            var message = "";

            foreach (var liability in Liabilities)
            {
                var name = TranslationService.Get(liability.Type.GetDescription(), CurrentUser);
                var fullAmount = liability.FullAmount;
                var cashflow = Math.Abs(liability.Cashflow);

                message += $"{Environment.NewLine}*{name}:* {fullAmount.AsCurrency()} - {cashflow.AsCurrency()} {monthly}";
            }

            return $"*{cashTerm}:* {person.Cash.AsCurrency()}{Environment.NewLine}{message}";
        }
    }

    public override IEnumerable<string> Buttons => Liabilities
        .Select(l => TranslationService.Get(l.Type.GetDescription(), CurrentUser))
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
            .FirstOrDefault(l => !l.Deleted && MessageEquals(message, l.Type.GetDescription()));

        if (liability is null)
        {
            return;
        }

        var minPaymentAmount = liability.AllowsPartialPayment ? 1_000 : liability.FullAmount;
        if (person.Cash < minPaymentAmount)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NotEnoughAmount, CurrentUser,
                minPaymentAmount.AsCurrency(),
                person.Cash.AsCurrency()));
            return;
        }

        liability.MarkedForReduction = true;
        PersonService.Update(CurrentUser, liability);

        NextStage = liability.AllowsPartialPayment ? New<ReduceLiabilitiesAmount>() : New<ReduceLiabilitiesConfirm>();
    }
}
