using CashFlow.Data.Consts;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BigOpportunity(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.WhatDoYouWant, CurrentUser);
    public override IEnumerable<string> Buttons =>
    [
        TranslationService.Get(Terms.BuyRealEstate, CurrentUser),
        TranslationService.Get(Terms.BuyBusiness, CurrentUser),
        TranslationService.Get(Terms.BuyLand, CurrentUser),
        Cancel
    ];

    public override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, "Buy Real Estate"):
                NextStage = New<BuyBigRealEstate>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, "Buy Business"):
                NextStage = New<BuyBusiness>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, "Buy Land"):
                NextStage = New<BuyLand>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, Terms.Cancel):
                NextStage = New<Start>();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}