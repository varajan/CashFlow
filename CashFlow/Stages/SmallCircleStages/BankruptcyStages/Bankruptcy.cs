using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class Bankruptcy(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => Terms.Get(129, CurrentUser, "You are bankrupt. Game is over.");

    public override IEnumerable<string> Buttons => [ StopGame, History ];

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
