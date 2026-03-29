using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetWithCashflowFirstPayment<TNextStage, TCreditStage>(
    AssetType assetName,
    AssetType assetType,
    ITranslationService termsService,
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
    
    public override string Message => TranslationService.Get(Terms.AskFirstPayment, CurrentUser);
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
        if (number <  0 && asset.Type != AssetType.BigBusinessType ||
            number <= 0 && asset.Type == AssetType.BigBusinessType)
        {
            await CurrentUser.Notify(TranslationService.Get("Invalid first payment value. Try again.", CurrentUser));
            NextStage = this;
            return;
        }

        asset.Mortgage = asset.Price - number;
        PersonService.UpdateAsset(CurrentUser, asset);

        var person = PersonService.Read(CurrentUser);
        if (person.Cash < number && asset.Type == AssetType.BigBusinessType)
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            await CurrentUser.Notify(TranslationService.Get(Terms.NotEnoughMoney, CurrentUser));
        }

        NextStage = person.Cash < number
            ? New<TCreditStage>()
            : New<TNextStage>();
    }
}
