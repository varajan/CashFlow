using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class Bankruptcy(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get("You are bankrupt. Game is over.", CurrentUser);

    public override IEnumerable<string> Buttons => [ StopGame, History ];

    public override Task HandleMessage(string message)
    {
        if (MessageEquals(message, "Stop Game"))
        {
            NextStage = New<StopGame>();
            return Task.CompletedTask;
        }

        if (MessageEquals(message, "History"))
        {
            NextStage = New<History>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
