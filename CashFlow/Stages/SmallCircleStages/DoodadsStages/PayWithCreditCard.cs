using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCreditCard(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager)
    : BaseStage(termsService, personManager)
{
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

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

        var person = PersonManager.Read(CurrentUser);
        person.UpdateLiability(Liability.Credit_Card, -(int)(amount * 0.03), amount);
        PersonManager.Update(person);
        PersonManager.AddHistory(ActionType.MicroCredit, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
