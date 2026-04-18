using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public class BuyAssetPriceWithCount<TNextStage>(
    AssetType assetName,
    AssetType assetType,
    ITranslationService termsService, IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(assetName, assetType, termsService, userService, availableAssets, personManager, userRepository) where TNextStage : BaseStage
{
    public override string Message => TranslationService.Get(Terms.AskPrice, CurrentUser);
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetName).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var asset = PersonService.ReadAllAssets(AssetType, CurrentUser).First(x => x.IsDraft);

        if (IsCanceled(message))
        {
            PersonService.DeleteAsset(CurrentUser, asset);
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidQty, CurrentUser));
            return;
        }

        asset.Price = number;
        PersonService.UpdateAsset(CurrentUser, asset);

        NextStage = New<TNextStage>();
    }
}
