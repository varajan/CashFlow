using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class Market(ITermsRepository termsService, IPersonService personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(89, CurrentUser, "What do you want?");

    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(38, CurrentUser, "Sell Real Estate"),
        Terms.Get(98, CurrentUser, "Sell Land"),
        Terms.Get(75, CurrentUser, "Sell Business"),
        Terms.Get(120, CurrentUser, "Sell Coins"),
        Terms.Get(118, CurrentUser, "Increase cash flow"),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var noSmallBusiness = PersonManager.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser).Count == 0;

        switch (message)
        {
            case var m when MessageEquals(m, 38, "Sell Real Estate"):
                var noRealEstate = PersonManager.ReadAllAssets(AssetType.RealEstate, CurrentUser).Count == 0;
                if (noRealEstate)
                {
                    await CurrentUser.Notify(Terms.Get(15, CurrentUser, "You have no properties."));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellRealEstate>();
                return;

            case var m when MessageEquals(m, 98, "Sell Land"):
                var noLand = PersonManager.ReadAllAssets(AssetType.Land, CurrentUser).Count == 0;
                if (noLand)
                {
                    await CurrentUser.Notify(Terms.Get(100, CurrentUser, "You have no Land."));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellLand>();
                return;

            case var m when MessageEquals(m, 75, "Sell Business"):
                var noBusiness = PersonManager.ReadAllAssets(AssetType.Business, CurrentUser).Count == 0;
                if (noBusiness && noSmallBusiness)
                {
                    await CurrentUser.Notify(Terms.Get(77, CurrentUser, "You have no Business."));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellBusiness>();
                return;

            case var m when MessageEquals(m, 120, "Sell Coins"):
                var noCoins = PersonManager.ReadAllAssets(AssetType.Coin, CurrentUser).Count == 0;
                if (noCoins)
                {
                    await CurrentUser.Notify(Terms.Get(121, CurrentUser, "You have no coins."));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellCoins>();
                return;

            case var m when MessageEquals(m, 118, "Increase cash flow"):
                if (noSmallBusiness)
                {
                    await CurrentUser.Notify(Terms.Get(136, CurrentUser, "You have no small Business."));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<IncreaseCashflow>();
                return;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}
