using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class GetMoney(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            return TranslationService.Get(Terms.CashflowAsk, CurrentUser,
                person.BigCircle
                ? person.GetBigCircleCashflow().AsCurrency()
                : person.GetSmallCircleCashflow().AsCurrency());
        }
    }

    public override IEnumerable<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            List<string> buttons = person.BigCircle
            ? [
                50_000.AsCurrency(),
                100_000.AsCurrency(),
                200_000.AsCurrency(),
                person.GetBigCircleCashflow().AsCurrency()
            ]
            : [
                1_000.AsCurrency(),
                2_000.AsCurrency(),
                5_000.AsCurrency(),
                person.GetSmallCircleCashflow().AsCurrency()
            ];

            return buttons.Distinct().Append(Cancel);
        }
    }

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var person = PersonService.Read(CurrentUser);
        var amount = message.AsCurrency();

        if (person.BigCircle && amount <=0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidValue, CurrentUser));
            return;
        }

        var bankruptcy = amount < 0 && person.Cash + amount < 0;
        if (bankruptcy)
        {
            await ProcessBankruptcy(person);
            return;
        }

        person.Cash += amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.GetMoney, amount, CurrentUser);

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.GotAmount, CurrentUser, amount.AsCurrency()));
        NextStage = New<Start>();
    }
}
