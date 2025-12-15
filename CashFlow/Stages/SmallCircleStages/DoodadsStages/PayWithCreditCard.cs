using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCreditCard(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager, IHistoryManager historyManager)
    : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.MicroCreditAmount).Append(Cancel);

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
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        var person = PersonManager.Read(CurrentUser.Id);
        person.Liabilities.CreditCard += amount;
        person.Expenses.CreditCard += (int)(amount * 0.03);
        PersonManager.Update(person);
        HistoryManager.Add(ActionType.MicroCredit, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
