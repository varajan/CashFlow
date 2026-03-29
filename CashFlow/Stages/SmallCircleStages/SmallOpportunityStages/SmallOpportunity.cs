using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class SmallOpportunity(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get("What do you want?", CurrentUser);
    public override IEnumerable<string> Buttons =>
    [
        TranslationService.Get(Terms.BuyStocks, CurrentUser),
        TranslationService.Get("Sell Stocks", CurrentUser),
        TranslationService.Get("Stocks x2", CurrentUser),
        TranslationService.Get("Stocks ÷2", CurrentUser),
        TranslationService.Get("Buy Real Estate", CurrentUser),
        TranslationService.Get("Buy Land", CurrentUser),
        TranslationService.Get(Terms.BuyCoins, CurrentUser),
        TranslationService.Get("Start a company", CurrentUser),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var hasStocks = PersonService.ReadAllAssets(AssetType.Stock, CurrentUser).Count(x => !x.IsDeleted) > 0;

        switch (message)
        {
            case var m when MessageEquals(m, "Buy Stocks"):
                NextStage = New<BuyStocks>();
                return;

            case var m when MessageEquals(m, "Sell Stocks"):
                if (hasStocks)
                {
                    NextStage = New<SellStocks>();
                    return;
                }

                await CurrentUser.Notify(TranslationService.Get("You have no stocks.", CurrentUser));
                return;

            case var m when MessageEquals(m, "Stocks x2"):
                if (hasStocks)
                {
                    NextStage = New<StocksMultiply>();
                    return;
                }

                await CurrentUser.Notify(TranslationService.Get("You have no stocks.", CurrentUser));
                return;

            case var m when MessageEquals(m, "Stocks ÷2"):
                if (hasStocks)
                {
                    NextStage = New<StocksReduce>();
                    return;
                }

                await CurrentUser.Notify(TranslationService.Get("You have no stocks.", CurrentUser));
                return;

            case var m when MessageEquals(m, "Buy Real Estate"):
                NextStage = New<BuySmallRealEstate>();
                return;

            case var m when MessageEquals(m, "Buy Land"):
                NextStage = New<BuyLand>();
                return;

            case var m when MessageEquals(m, Terms.BuyCoins):
                NextStage = New<BuyCoins>();
                return;

            case var m when MessageEquals(m, "Start a company"):
                NextStage = New<StartCompany>();
                return;

            case var m when MessageEquals(m, "Cancel"):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}