using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using System.Text;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

public class StartCompanyPrice(ITermsService termsService, IAvailableAssets assets) : StartCompany(termsService, assets)
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessBuyPrice, CurrentUser.Language).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return;

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        Asset.Price = number;

        if (CurrentUser.Person_OBSOLETE.Cash < number)
        {
            NextStage = New<StartCompanyCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        CurrentUser.Person_OBSOLETE.Cash -= Asset.Price;
        CurrentUser.History_OBSOLETE.Add(ActionType.StartCompany, Asset.Id);
        Asset.IsDraft = false;

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}
