using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class SmallOpportunity(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);
    public override List<List<string>> ButtonsAsMatrix
    {
        get
        {
            var buyStocks = TranslationService.Get(Terms.BuyStocks, CurrentUser);
            var sellStocks = TranslationService.Get(Terms.SellStocks, CurrentUser);
            var stocksX2 = TranslationService.Get(Terms.StocksX2, CurrentUser);
            var stocksDiv2 = TranslationService.Get(Terms.StocksDiv2, CurrentUser);
            var buyRealEstate = TranslationService.Get(Terms.BuyRealEstate, CurrentUser);
            var buyLand = TranslationService.Get(Terms.BuyLand, CurrentUser);
            var buyCoins = TranslationService.Get(Terms.BuyCoins, CurrentUser);
            var startCompany = TranslationService.Get(Terms.StartCompany, CurrentUser);

            return
            [
                [buyStocks, sellStocks, stocksX2, stocksDiv2],
                [buyRealEstate, buyLand],
                [ buyCoins, startCompany],
                [Cancel],
            ];
        }
    }

    public override async Task HandleMessage(string message)
    {
        var hasStocks = PersonService.ReadActiveAssets(AssetType.Stock, CurrentUser).Count > 0;

        switch (message)
        {
            case var m when MessageEquals(m, Terms.BuyStocks):
                NextStage = New<BuyStocks>();
                return;

            case var m when MessageEquals(m, Terms.SellStocks):
                if (hasStocks)
                {
                    NextStage = New<SellStocks>();
                    return;
                }

                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoStocks, CurrentUser));
                return;

            case var m when MessageEquals(m, Terms.StocksX2):
                if (hasStocks)
                {
                    NextStage = New<StocksMultiply>();
                    return;
                }

                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoStocks, CurrentUser));
                return;

            case var m when MessageEquals(m, Terms.StocksDiv2):
                if (hasStocks)
                {
                    NextStage = New<StocksReduce>();
                    return;
                }

                await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoStocks, CurrentUser));
                return;

            case var m when MessageEquals(m, Terms.BuyRealEstate):
                NextStage = New<BuySmallRealEstate>();
                return;

            case var m when MessageEquals(m, Terms.BuyLand):
                NextStage = New<BuyLand>();
                return;

            case var m when MessageEquals(m, Terms.BuyCoins):
                NextStage = New<BuyCoins>();
                return;

            case var m when MessageEquals(m, Terms.StartCompany):
                NextStage = New<StartCompany>();
                return;

            case var m when MessageEquals(m, Terms.Cancel):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}