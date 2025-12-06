using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

namespace CashFlow.Stages.BuyRealEstateStages;

public abstract class BuyRealEstatePrice(
    bool small,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IAssetManager assetManager) : BuyRealEstate(small, termsService, availableAssets, assetManager)
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.RealEstateSmallBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = AssetManager.ReadAll(AssetType.RealEstate, CurrentUser.Id).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            AssetManager.Delete(asset);
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
        AssetManager.Update(asset);
        NextStage = IsSmall ? New<BuySmallRealEstateFirstPayment>() : throw new NotImplementedException();
    }
}
