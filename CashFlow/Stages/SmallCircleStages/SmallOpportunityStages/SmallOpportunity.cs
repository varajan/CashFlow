using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class SmallOpportunity(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(89, CurrentUser, "What do you want?");
    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(35, CurrentUser, "Buy Stocks"),
        Terms.Get(36, CurrentUser, "Sell Stocks"),
        Terms.Get(82, CurrentUser, "Stocks x2"),
        Terms.Get(83, CurrentUser, "Stocks ÷2"),
        Terms.Get(37, CurrentUser, "Buy Real Estate"),
        Terms.Get(94, CurrentUser, "Buy Land"),
        Terms.Get(119, CurrentUser, "Buy coins"),
        Terms.Get(115, CurrentUser, "Start a company"),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var hasStocks = PersonManager.ReadAllAssets(AssetType.Stock, CurrentUser.Id).Count > 0;

        switch (message)
        {
            case var m when MessageEquals(m, 35, "Buy Stocks"):
                NextStage = New<BuyStocks>();
                return;

            case var m when MessageEquals(m, 36, "Sell Stocks"):
                if (hasStocks)
                {
                    NextStage = New<SellStocks>();
                    return;
                }

                await CurrentUser.Notify(Terms.Get(49, CurrentUser, "You have no stocks."));
                return;

            case var m when MessageEquals(m, 82, "Stocks x2"):
                if (hasStocks)
                {
                    NextStage = New<StocksMultiply>();
                    return;
                }

                await CurrentUser.Notify(Terms.Get(49, CurrentUser, "You have no stocks."));
                return;

            case var m when MessageEquals(m, 83, "Stocks ÷2"):
                if (hasStocks)
                {
                    NextStage = New<StocksReduce>();
                    return;
                }

                await CurrentUser.Notify(Terms.Get(49, CurrentUser, "You have no stocks."));
                return;

            case var m when MessageEquals(m, 37, "Buy Real Estate"):
                NextStage = New<BuySmallRealEstate>();
                return;

            case var m when MessageEquals(m, 94, "Buy Land"):
                NextStage = New<BuyLand>();
                return;

            case var m when MessageEquals(m, 119, "Buy coins"):
                NextStage = New<BuyCoins>();
                return;

            case var m when MessageEquals(m, 115, "Start a company"):
                NextStage = New<StartCompany>();
                return;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}