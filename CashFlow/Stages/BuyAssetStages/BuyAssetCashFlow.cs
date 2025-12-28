using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetCashFlow<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager,
    IHistoryManager historyManager,
    IPersonManager personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, assetManager, personManager) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Terms.Get(12, CurrentUser, "What is the cash flow?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType, CurrentUser.Id).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
            NextStage = New<Start>();
            return;
        }

        asset.CashFlow = message.AsCurrency();
        asset.IsDraft = false;
        AssetManager.Update(asset);
        
        var person = PersonManager.Read(CurrentUser.Id);
        var amount = asset.Price * asset.Qtty - asset.Mortgage;
        person.Cash -= amount;
        PersonManager.Update(person);
        HistoryManager.Add(ActionType, asset.Id, CurrentUser);
        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));

        NextStage = New<Start>();
    }
}
