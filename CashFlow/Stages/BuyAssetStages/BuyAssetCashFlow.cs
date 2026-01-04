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
    IPersonManager personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    public override string Message => Terms.Get(12, CurrentUser, "What is the cash flow?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(asset);
            NextStage = New<Start>();
            return;
        }

        asset.CashFlow = message.AsCurrency();
        asset.IsDraft = false;
        PersonManager.UpdateAsset(asset);
        
        var person = PersonManager.Read(CurrentUser);
        var amount = asset.Price * asset.Qtty - asset.Mortgage;
        person.Cash -= amount;
        PersonManager.Update(person);
        PersonManager.AddHistory(ActionType, asset.Id, CurrentUser);
        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));

        NextStage = New<Start>();
    }
}
