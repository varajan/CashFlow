using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowFirstPayment<TNextStage, TCreditStage>(
    AssetType assetName,
    AssetType assetType,
    ITermsRepository termsService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
     : BaseStage(termsService, personManager, userRepository)
        where TNextStage : BaseStage
        where TCreditStage : BaseStage
{
    protected AssetType AssetName { get; } = assetName;
    protected AssetType AssetType { get; } = assetType;
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;
    
    public override string Message => Terms.Get(10, CurrentUser, "What is the first payment?");
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
        if (number <  0 && asset.Type != AssetType.BigBusinessType ||
            number <= 0 && asset.Type == AssetType.BigBusinessType)
        {
            await CurrentUser.Notify(Terms.Get(11, CurrentUser, "Invalid first payment value. Try again."));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        PersonManager.UpdateAsset(CurrentUser, asset);

        var person = PersonManager.Read(CurrentUser);
        if (person.Cash < number && asset.Type == AssetType.BigBusinessType)
        {
            PersonManager.DeleteAsset(CurrentUser, asset);
            await CurrentUser.Notify(Terms.Get(5, CurrentUser, "You don't have enough money."));
        }

        NextStage = person.Cash < number
            ? New<TCreditStage>()
            : New<TNextStage>();
    }
}
