using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowFirstPayment<TNextStage, TCreditStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager)
     : BaseStage(termsService, personManager)
        where TNextStage : BaseStage
        where TCreditStage : BaseStage
{
    protected AssetType AssetName { get; } = assetName;
    protected AssetType AssetType { get; } = assetType;
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;
    
    public override string Message => Terms.Get(10, CurrentUser, "What is the first payment?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var asset = PersonManager.ReadAllAssets(AssetType, CurrentUser.Id).Single(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonManager.DeleteAsset(asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number <  0 && asset.Type != AssetType.BigBusinessType ||
            number <= 0 && asset.Type == AssetType.BigBusinessType)
        {
            await CurrentUser.Notify(Terms.Get(11, CurrentUser, "Invalid first payment value. Try again."));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        PersonManager.UpdateAsset(asset);

        var person = PersonManager.Read(CurrentUser.Id);
        if (person.Cash < number && asset.Type == AssetType.BigBusinessType)
        {
            PersonManager.DeleteAsset(asset);
            await CurrentUser.Notify(Terms.Get(5, CurrentUser, "You don't have enough money."));
        }

        NextStage = person.Cash < number
            ? New<TCreditStage>()
            : New<TNextStage>();
    }
}
