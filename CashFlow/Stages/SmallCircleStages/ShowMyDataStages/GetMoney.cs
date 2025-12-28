using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class GetMoney(ITermsService termsService, IPersonManager personManager, IHistoryManager historyManager)
    : BaseStage(termsService, personManager)
{
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(0, CurrentUser,
        "Your Cash Flow is *{0}*. How much should you get?",
        PersonManager.Read(CurrentUser.Id).CashFlow);

    public override IEnumerable<string> Buttons
    {
        get
        {
            var buttons = new List<string>
            {
                1000.AsCurrency(),
                2000.AsCurrency(),
                5000.AsCurrency(),
                PersonManager.Read(CurrentUser.Id).CashFlow.AsCurrency()
            };
 
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

        var person = PersonManager.Read(CurrentUser.Id);
        var amount = message.AsCurrency();
        var isBankruptcy = await IsBankruptcy(person, amount);
        if (isBankruptcy) return;

        person.Cash += amount;
        PersonManager.Update(person);
        HistoryManager.Add(ActionType.GetMoney, amount, CurrentUser);

        await CurrentUser.Notify(Terms.Get(22, CurrentUser, "Ok, you've got *{0}*", amount.AsCurrency()));
        NextStage = New<Start>();
    }
}
