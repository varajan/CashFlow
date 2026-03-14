using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITermsRepository termsService, IPersonService personManager)
    : ConfirmStage(termsService, personManager, 3, "Are you sure want to stop current game?")
{
    protected override Task OnConfirmed()
    {
        PersonManager.Delete(CurrentUser);
        NextStage = New<Start>();

        return Task.CompletedTask;
    }
}
