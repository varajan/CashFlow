using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class GetCredit(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskHowMany, CurrentUser);
    public override IEnumerable<string> Buttons => ["1000", "2000", "5000", "10 000", "20 000", Cancel];

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number % 1000 > 0 || number < 1000)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidAmount, CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);
        person.GetCredit(number);
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Credit, number, CurrentUser);
        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.TookLoan, CurrentUser, number.AsCurrency()));
        NextStage = New<Start>();
    }
}
