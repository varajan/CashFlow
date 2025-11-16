using CashFlow.Data;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.BuyCoinsStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages.StartCompanyStages;

namespace CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

public class SmallOpportunity(ITermsService termsService) : BaseStage(termsService)
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

    public override Task HandleMessage(string message)
    {
        CurrentUser.Person_OBSOLETE.Assets.CleanUp();

        switch (message)
        {
            case var m when MessageEquals(m, 35, "Buy Stocks"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 36, "Sell Stocks"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 82, "Stocks x2"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 83, "Stocks ÷2"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 37, "Buy Real Estate"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 94, "Buy Land"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 119, "Buy coins"):
                NextStage = New<BuyCoins>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 115, "Start a company"):
                NextStage = New<StartCompany>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}