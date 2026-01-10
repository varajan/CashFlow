using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITermsService termsService, IPersonManager personManager)
    : ConfirmStage(termsService, personManager, 3, "Are you sure want to stop current game?")
{
    protected override Task OnConfirmed()
    {
        PersonManager.ClearHistory(CurrentUser);
        PersonManager.Delete(CurrentUser);
        NextStage = New<Start>();

        return Task.CompletedTask;
    }
}
