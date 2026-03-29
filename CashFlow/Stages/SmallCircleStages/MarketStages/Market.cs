using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class Market(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get("What do you want?", CurrentUser);

    public override IEnumerable<string> Buttons =>
    [
        Terms.Get("Sell Real Estate", CurrentUser),
        Terms.Get("Sell Land", CurrentUser),
        Terms.Get("Sell Business", CurrentUser),
        Terms.Get("Sell Coins", CurrentUser),
        Terms.Get("Increase cash flow", CurrentUser),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var noSmallBusiness = PersonService.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser).Count == 0;

        switch (message)
        {
            case var m when MessageEquals(m, "Sell Real Estate"):
                var noRealEstate = PersonService.ReadAllAssets(AssetType.RealEstate, CurrentUser).Count == 0;
                if (noRealEstate)
                {
                    await CurrentUser.Notify(Terms.Get("You have no properties.", CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellRealEstate>();
                return;

            case var m when MessageEquals(m, "Sell Land"):
                var noLand = PersonService.ReadAllAssets(AssetType.Land, CurrentUser).Count == 0;
                if (noLand)
                {
                    await CurrentUser.Notify(Terms.Get("You have no Land.", CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellLand>();
                return;

            case var m when MessageEquals(m, "Sell Business"):
                var noBusiness = PersonService.ReadAllAssets(AssetType.Business, CurrentUser).Count == 0;
                if (noBusiness && noSmallBusiness)
                {
                    await CurrentUser.Notify(Terms.Get("You have no Business.", CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellBusiness>();
                return;

            case var m when MessageEquals(m, "Sell Coins"):
                var noCoins = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).Count == 0;
                if (noCoins)
                {
                    await CurrentUser.Notify(Terms.Get("You have no coins.", CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellCoins>();
                return;

            case var m when MessageEquals(m, "Increase cash flow"):
                if (noSmallBusiness)
                {
                    await CurrentUser.Notify(Terms.Get("You have no small Business.", CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<IncreaseCashflow>();
                return;

            case var m when MessageEquals(m, "Cancel"):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}
