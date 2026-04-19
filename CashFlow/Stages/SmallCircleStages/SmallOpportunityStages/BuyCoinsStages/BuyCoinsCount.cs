using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsCount(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BuyCoins(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.AskHowMany, CurrentUser);

    public override IEnumerable<string> Buttons => ["1", "10", Cancel];

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

        var asset = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft && !x.IsDeleted);
        asset.Qtty = number;
        PersonService.UpdateAsset(CurrentUser, asset);

        NextStage = New<BuyCoinsPrice>();
    }
}
