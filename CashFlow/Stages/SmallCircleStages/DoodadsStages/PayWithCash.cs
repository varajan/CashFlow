using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCash(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskHowMany, CurrentUser);

    public override IEnumerable<string> Buttons => Prices.SmallGiveMoney.OrderBy(x => x).AsCurrency().Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();
        if (amount <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidValue, CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);

        if (person.Cash < amount)
        {
            var delta = amount - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            person.GetCredit(credit);
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.TookLoan, CurrentUser, credit.AsCurrency()));
        }

        person.Cash -= amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.PayMoney, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
