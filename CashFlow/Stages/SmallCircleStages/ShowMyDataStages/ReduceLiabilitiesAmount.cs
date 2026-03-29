using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;
using System.Text;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilitiesAmount(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get("How much?", CurrentUser);

    public override IEnumerable<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var liability = PersonService.Read(CurrentUser).Liabilities.FirstOrDefault(l => l.MarkedForReduction);
            var buttons = new[] { 1000, 5000, 10000, liability.FullAmount, person.Cash / 1000 * 1000 }
                .Where(x => x <= person.Cash && x <= liability.FullAmount)
                .OrderBy(x => x)
                .Distinct()
                .Select(x => x.AsCurrency());

            return buttons.Append(Cancel);
        }
    }

    public override async Task HandleMessage(string message)
    {
        var person = PersonService.Read(CurrentUser);

        if (IsCanceled(message))
        {
            person.Liabilities
                .Where(liability => liability.MarkedForReduction)
                .ForEach(liability =>
                {
                    liability.MarkedForReduction = false;
                    PersonService.Update(CurrentUser, liability);
                });

            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();
        if (amount % 1000 > 0 || amount < 1000)
        {
            await CurrentUser.Notify(Terms.Get("Invalid amount. The amount must be a multiple of 1000", CurrentUser));
            return;
        }

        if (amount > person.Cash)
        {
            await CurrentUser.Notify(Terms.Get("You don't have {0}, but only {1}", CurrentUser, amount.AsCurrency(), person.Cash.AsCurrency()));
            return;
        }

        ReduceLiability(amount, person);

        NextStage = PersonService.Read(CurrentUser).Liabilities.All(l => l.FullAmount == 0)
            ? New<Start>()
            : New<ReduceLiabilities>();
    }

    private void ReduceLiability(int amount, PersonDto person)
    {
        var liability = PersonService.Read(CurrentUser).Liabilities.FirstOrDefault(l => l.MarkedForReduction);
        amount = amount / 1000 * 1000;
        amount = Math.Min(amount, liability.FullAmount);
        var percent = (decimal)1 / 10;
        var cashflow = (int)(amount * percent);

        person.Cash -= amount;
        liability.Cashflow += cashflow;
        liability.FullAmount -= amount;
        liability.MarkedForReduction = false;
        liability.Deleted = liability.FullAmount == 0;

        PersonService.Update(person);
        PersonService.Update(CurrentUser, liability);
        PersonService.AddHistory((ActionType)liability.Type, amount, CurrentUser);
    }
}
