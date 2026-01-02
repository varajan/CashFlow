using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class IncreaseCashflow(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager) : BaseStage(termsService, personManager)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    protected IAssetManager AssetManager { get; } = assetManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

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

        var assets = PersonManager.ReadAllAssets(AssetType.SmallBusiness, CurrentUser.Id);
        assets.ForEach(asset =>
        {
            asset.CashFlow += cashflow;
            PersonManager.UpdateAsset(asset);
            HistoryManager.Add(ActionType.IncreaseCashFlow, asset.Id, CurrentUser);
        });

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
        NextStage = New<Start>();
    }
}
