using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class Market(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);

    public override IEnumerable<string> Buttons =>
    [
        TranslationService.Get(Terms.SellRealEstate, CurrentUser),
        TranslationService.Get(Terms.SellLand, CurrentUser),
        TranslationService.Get(Terms.SellBusiness, CurrentUser),
        TranslationService.Get(Terms.SellCoins, CurrentUser),
        TranslationService.Get(Terms.IncreaseCashflow, CurrentUser),
        Cancel
    ];

    public override async Task HandleMessage(string message)
    {
        var noSmallBusiness = PersonService.ReadAllAssets(AssetType.SmallBusinessType, CurrentUser).Count == 0;

        switch (message)
        {
            case var m when MessageEquals(m, Terms.SellRealEstate):
                var noRealEstate = PersonService.ReadAllAssets(AssetType.RealEstate, CurrentUser).Count == 0;
                if (noRealEstate)
                {
                    await CurrentUser.Notify(TranslationService.Get(Terms.NoRealEstate, CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellRealEstate>();
                return;

            case var m when MessageEquals(m, Terms.SellLand):
                var noLand = PersonService.ReadAllAssets(AssetType.Land, CurrentUser).Count == 0;
                if (noLand)
                {
                    await CurrentUser.Notify(TranslationService.Get(Terms.NoLand, CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellLand>();
                return;

            case var m when MessageEquals(m, Terms.SellBusiness):
                var noBusiness = PersonService.ReadAllAssets(AssetType.Business, CurrentUser).Count == 0;
                if (noBusiness && noSmallBusiness)
                {
                    await CurrentUser.Notify(TranslationService.Get(Terms.NoBusiness, CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellBusiness>();
                return;

            case var m when MessageEquals(m, Terms.SellCoins):
                var noCoins = PersonService.ReadAllAssets(AssetType.Coin, CurrentUser).Count == 0;
                if (noCoins)
                {
                    await CurrentUser.Notify(TranslationService.Get(Terms.NoCoins, CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<SellCoins>();
                return;

            case var m when MessageEquals(m, Terms.IncreaseCashflow):
                if (noSmallBusiness)
                {
                    await CurrentUser.Notify(TranslationService.Get(Terms.NoSmallBusiness, CurrentUser));
                    NextStage = New<Start>();
                    return;
                }

                NextStage = New<IncreaseCashflow>();
                return;

            case var m when MessageEquals(m, Terms.Cancel):
                NextStage = New<Start>();
                return;
        }

        return;
    }
}
