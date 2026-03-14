using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BigOpportunity(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get(89, CurrentUser, "What do you want?");
    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(37, CurrentUser, "Buy Real Estate"),
        Terms.Get(74, CurrentUser, "Buy Business"),
        Terms.Get(94, CurrentUser, "Buy Land"),
        Cancel
    ];

    public override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 37, "Buy Real Estate"):
                NextStage = New<BuyBigRealEstate>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 74, "Buy Business"):
                NextStage = New<BuyBusiness>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 94, "Buy Land"):
                NextStage = New<BuyLand>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}