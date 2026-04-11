using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : ConfirmStage(termsService, userService, personManager, userRepository, "Are you sure want to stop current game?")
{
    protected override Task OnConfirmed()
    {
        PersonService.Delete(CurrentUser);
        NextStage = New<Start>();

        return Task.CompletedTask;
    }
}
