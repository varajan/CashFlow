using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ShowMyData(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => PersonService.GetDescription(CurrentUser, false);

    public override List<string> Buttons =>
    [
        TranslationService.Get(Terms.GetMoney, CurrentUser),
        TranslationService.Get(Terms.GetCredit, CurrentUser),
        TranslationService.Get(Terms.Charity10, CurrentUser),
        TranslationService.Get(Terms.ReduceLiabilities, CurrentUser),
        TranslationService.Get(Terms.MainMenu, CurrentUser),
    ];

    public async override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, Terms.GetMoney):
                NextStage = New<GetMoney>();
                return;

            case var m when MessageEquals(m, Terms.GetCredit):
                NextStage = New<GetCredit>();
                return;

            case var m when MessageEquals(m, Terms.Charity10):
                await Charity();
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, Terms.ReduceLiabilities):
                await ReduceLiabilities();
                return;

            case var m when MessageEquals(m, Terms.MainMenu):
                NextStage = New<Start>();
                return;
        }
    }

    private async Task ReduceLiabilities()
    {
        var person = PersonService.Read(CurrentUser);
        if (person.Liabilities.Any(l => l.FullAmount > 0))
        {
            NextStage = New<ReduceLiabilities>();
            return;
        }

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoLiabilities, CurrentUser));
        NextStage = New<Start>();
    }

    private async Task Charity()
    {
        var person = PersonService.Read(CurrentUser);
        var amount = (person.Assets.Sum(a => a.CashFlow) + person.Salary) / 10;

        if (person.Cash < amount)
        {
            var notEnoughCashMsg = TranslationService.Get(Terms.NotEnoughAmount, CurrentUser, amount.AsCurrency(), person.Cash.AsCurrency());
            await UserService.Notify(CurrentUser, notEnoughCashMsg);
            return;
        }

        person.Cash -= amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Charity, amount, CurrentUser);

        var message = TranslationService.Get(Terms.CharityResult, CurrentUser, amount.AsCurrency());
        await UserService.Notify(CurrentUser, message);
    }
}