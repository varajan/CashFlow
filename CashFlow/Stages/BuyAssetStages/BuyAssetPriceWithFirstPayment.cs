using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BuyAssetStages;

public abstract class BuyAssetPriceWithFirstPayment<TNextStage>(
    int[] assetPrices,
    AssetType assetType,
    ITranslationService termsService,
    IUserService userService,
    IPersonService personManager,
    IUserRepository userRepository)
    : BuyAsset<TNextStage>(null, assetType, termsService, userService, personManager, userRepository) where TNextStage : BaseStage
{
    public override string Message => TranslationService.Get(Terms.AskPrice, CurrentUser);
    public override IEnumerable<string> Buttons => assetPrices.AsCurrency().Append(Cancel);

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
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidPrice, CurrentUser));
            return;
        }

        asset.Price = number;
        PersonService.UpdateAsset(CurrentUser, asset);
        NextStage = New<TNextStage>();
    }
}
