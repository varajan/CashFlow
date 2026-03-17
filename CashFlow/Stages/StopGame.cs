using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : ConfirmStage(termsService, personManager, userRepository, 3, "Are you sure want to stop current game?")
{
    protected override Task OnConfirmed()
    {
        PersonService.Delete(CurrentUser);
        NextStage = New<Start>();

        return Task.CompletedTask;
    }
}
