using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class Bankruptcy(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(129, CurrentUser, "You are bankrupt. Game is over.");

    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(41, CurrentUser, "Stop Game"),
        Terms.Get(2, CurrentUser, "History")
    ];

    public override Task HandleMessage(string message)
    {
        if (MessageEquals(message, 41, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, 2, "History"))
        {
            NextStage = New<History>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
