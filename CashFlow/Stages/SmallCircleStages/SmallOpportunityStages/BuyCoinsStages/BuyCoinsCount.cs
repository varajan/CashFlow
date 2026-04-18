using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsCount(
    ITranslationService termsService,
    IUserService userService,
    IAvailableAssetsRepository availableAssets,
    IPersonService personManager,
    IUserRepository userRepository) : BuyCoins(termsService, userService, availableAssets, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskHowMany, CurrentUser);

    public override IEnumerable<string> Buttons => AvailableAssets
        .GetAsText(AssetType.CoinCount, CurrentUser.Language)
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();

        if (number <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidQty, CurrentUser));
            return;
        }

        var asset = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);
        asset.Qtty = number;
        PersonService.UpdateAsset(CurrentUser, asset);

        NextStage = New<BuyCoinsPrice>();
    }
}
