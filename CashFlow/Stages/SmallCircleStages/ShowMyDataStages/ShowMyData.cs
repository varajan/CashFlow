using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ShowMyData(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => PersonService.GetDescription(CurrentUser, false);

    public override List<string> Buttons =>
    [
        TranslationService.Get(Terms.GetMoney, CurrentUser),
        TranslationService.Get("Get Credit", CurrentUser),
        TranslationService.Get("Charity - Pay 10%", CurrentUser),
        TranslationService.Get("Reduce Liabilities", CurrentUser),
        StopGame,
        TranslationService.Get("Main menu", CurrentUser),
    ];

    public async override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, "Get Money"):
                NextStage = New<GetMoney>();
                return;

            case var m when MessageEquals(m, "Get Credit"):
                NextStage = New<GetCredit>();
                return;

            case var m when MessageEquals(m, "Charity - Pay 10%"):
                await Charity();
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, "Reduce Liabilities"):
                await ReduceLiabilities();
                return;

            case var m when MessageEquals(m, "Stop Game"):
                NextStage = New<StopGame>();
                return;

            case var m when MessageEquals(m, "Main menu"):
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

        await CurrentUser.Notify(TranslationService.Get("You have no liabilities.", CurrentUser));
        NextStage = New<Start>();
    }

    private async Task Charity()
    {
        var person = PersonService.Read(CurrentUser);
        var amount = (person.Assets.Sum(a => a.CashFlow) + person.Salary) / 10;

        if (person.Cash < amount)
        {
            var notEnoughCashMsg = TranslationService.Get(Terms.NotEnoughAmount, CurrentUser, amount.AsCurrency(), person.Cash.AsCurrency());
            await CurrentUser.Notify(notEnoughCashMsg);
            return;
        }

        person.Cash -= amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Charity, amount, CurrentUser);

        var message = TranslationService.Get("You've payed {0}, now you can use two dice in next 3 turns.", CurrentUser, amount.AsCurrency());
        await CurrentUser.Notify(message);
    }
}