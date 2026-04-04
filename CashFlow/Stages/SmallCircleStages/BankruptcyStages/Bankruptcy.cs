using CashFlow.Data.Consts;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.BankruptcyStages;

public class Bankruptcy(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(Terms.GameOver, CurrentUser);

    public override IEnumerable<string> Buttons => [ StopGame, History ];

    public override Task HandleMessage(string message)
    {
        if (MessageEquals(message, Terms.StopGame))
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
