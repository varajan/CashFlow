using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetPriceWithFirstPayment<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITranslationService termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    public override string Message => Terms.Get("What is the price?", CurrentUser);
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get("Invalid price value. Try again.", CurrentUser));
            return;
        }

        asset.Price = number;
        PersonService.UpdateAsset(CurrentUser, asset);
        NextStage = New<TNextStage>();
    }
}
