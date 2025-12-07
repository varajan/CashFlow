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
    IAssetManager assetManager) : BuyAsset<TNextStage>(assetName, assetType, termsService, availableAssets, assetManager) where TNextStage : BaseStage
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetName, CurrentUser.Language).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType, CurrentUser.Id).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
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
        AssetManager.Update(asset);

        NextStage = New<TNextStage>();
    }
}
