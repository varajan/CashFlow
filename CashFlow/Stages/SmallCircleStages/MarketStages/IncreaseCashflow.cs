using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class IncreaseCashflow(
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager) : BaseStage(termsService, personManager)
{
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(12, CurrentUser, "What is the cash flow?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.IncreaseCashFlow).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var cashflow = message.AsCurrency();
        if (cashflow <= 0)
        {
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        var assets = PersonManager.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser).Where(a => !a.IsDeleted);

        assets.ForEach(asset =>
        {
            asset.CashFlow += cashflow;
            PersonManager.UpdateAsset(CurrentUser, asset);
            PersonManager.AddHistory(ActionType.IncreaseCashFlow, cashflow, CurrentUser, asset.Id);
        });

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
