using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public class BuyAssetPriceWithCount<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, personManager) where TNextStage : BaseStage
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetName, CurrentUser.Language).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(18, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        asset.Price = number;
        PersonManager.UpdateAsset(asset);

        NextStage = New<TNextStage>();
    }
}
