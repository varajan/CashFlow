using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetPrice<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ActionType actionType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    protected ActionType ActionType { get; } = actionType;
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        asset.Price = number;
        PersonManager.UpdateAsset(CurrentUser, asset);

        var person = PersonManager.Read(CurrentUser);
        if (person.Cash < asset.Price)
        {
            NextStage = New<TNextStage>();
            return;
        }

        await CompleteTransaction(asset);
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction(AssetDto asset)
    {
        var person = PersonManager.Read(CurrentUser);

        person.Cash -= asset.Price;
        PersonManager.Update(person);

        asset.IsDraft = false;
        PersonManager.UpdateAsset(CurrentUser, asset);

        PersonManager.AddHistory(ActionType, asset.Price, CurrentUser, asset.Id);

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}
