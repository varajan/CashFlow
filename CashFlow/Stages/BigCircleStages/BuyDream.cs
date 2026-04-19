using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BigCircleStages;

public class BuyDream(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskPrice, CurrentUser);
    public override IEnumerable<string> Buttons => BuyPrice.BigDream.OrderBy(x => x).AsCurrency().Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidPrice, CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);
        if (person.Cash < number)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NotEnoughMoney, CurrentUser));
            NextStage = New<Start>();
            return;
        }

        person.Cash -= number;
        person.BoughtDream = true;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.BuyDream, number, CurrentUser);

        NextStage = New<Start>();
    }

}
