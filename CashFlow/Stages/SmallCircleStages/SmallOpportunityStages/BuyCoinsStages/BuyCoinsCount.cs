using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;

public class BuyCoinsCount(
    ITermsService termsService,
    IAvailableAssets availableAssets,
    IPersonManager personManager) : BuyCoins(termsService, availableAssets, personManager)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

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
            await CurrentUser.Notify(Terms.Get(18, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        var asset = PersonManager.ReadAllAssets(AssetType.Coin, CurrentUser).First(x => x.IsDraft);
        asset.Qtty = number;
        PersonManager.UpdateAsset(CurrentUser, asset);

        NextStage = New<BuyCoinsPrice>();
    }
}
