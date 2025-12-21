using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ShowMyData(ITermsService termsService, IPersonManager personManager, IHistoryManager historyManager) : BaseStage(termsService)
{
    protected IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => PersonManager.GetDescription(CurrentUser.Id);

    public override List<string> Buttons =>
    [
        Terms.Get(32, CurrentUser, "Get Money"),
        Terms.Get(34, CurrentUser, "Get Credit"),
        Terms.Get(90, CurrentUser, "Charity - Pay 10%"),
        Terms.Get(40, CurrentUser, "Reduce Liabilities"),
        Terms.Get(41, CurrentUser, "Stop Game"),
        Terms.Get(102, CurrentUser, "Main menu"),
    ];

    public async override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 32, "Get Money"):
                NextStage = New<GetMoney>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                NextStage = New<GetCredit>();
                return;

            case var m when MessageEquals(m, 90, "Charity - Pay 10%"):
                await Charity();
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 40, "Reduce Liabilities"):
                NextStage = New<ReduceLiabilities>();
                return;

            case var m when MessageEquals(m, 40, "Stop Game"):
                NextStage = New<StopGame>();
                return;

            case var m when MessageEquals(m, 102, "Main menu"):
                NextStage = New<Start>();
                return;
        }
    }

    private async Task Charity()
    {
        var person = PersonManager.Read(CurrentUser.Id);
        var amount = (person.Assets.Sum(a => a.CashFlow) + person.Salary) / 10;

        if (person.Cash < amount)
        {
            var notEnoughCashMsg = Terms.Get(23, CurrentUser, "You don't have {0}, but only {1}", amount.AsCurrency(), person.Cash.AsCurrency());
            await CurrentUser.Notify(notEnoughCashMsg);
            return;
        }

        person.Cash -= amount;
        PersonManager.Update(person);
        HistoryManager.Add(ActionType.Charity, amount, CurrentUser);

        var message = Terms.Get(91, CurrentUser, "You've payed {0}, now you can use two dice in next 3 turns.", amount.AsCurrency());
        await CurrentUser.Notify(message);
    }
}