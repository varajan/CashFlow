using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StocksStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class SmallOpportunity(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);
    public override IEnumerable<string> Buttons =>
    [
        TranslationService.Get(Terms.BuyStocks, CurrentUser),
        TranslationService.Get(Terms.SellStocks, CurrentUser),
        TranslationService.Get(Terms.StocksX2, CurrentUser),
        TranslationService.Get(Terms.StocksDiv2, CurrentUser),
        TranslationService.Get(Terms.BuyRealEstate, CurrentUser),
        TranslationService.Get(Terms.BuyLand, CurrentUser),
        TranslationService.Get(Terms.BuyCoins, CurrentUser),
        TranslationService.Get(Terms.StartCompany, CurrentUser),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var hasStocks = PersonService.ReadAllAssets(AssetType.Stock, CurrentUser).Count(x => !x.IsDeleted) > 0;

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