using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class StopGame(ITermsService termsService, IPersonManager personManager, IHistoryManager historyManager)
    : ConfirmStage(termsService, 3, "Are you sure want to stop current game?")
{
    protected IPersonManager PersonManager { get; } = personManager;
    protected IHistoryManager HistoryManager { get; } = historyManager;

    protected override Task OnConfirmed()
    {
        HistoryManager.Clear(CurrentUser.Id);
        PersonManager.Delete(CurrentUser.Id);
        return Task.CompletedTask;
    }
}

public abstract class ConfirmStage(ITermsService termsService, int id, string question) : BaseStage(termsService)
{
    public override string Message => Terms.Get(id, CurrentUser, question);
    public override List<string> Buttons => [ Yes, Cancel ];

    public async override Task HandleMessage(string message)
    {
        if (IsConfirmed(message))
        {
            await OnConfirmed();
        }
        else
        {
            await OnDismiss();
        }
    }

    protected bool IsConfirmed(string message) => MessageEquals(message, 4, "Yes");
    
    protected abstract Task OnConfirmed();

    protected virtual async Task OnDismiss()
    {
        NextStage = New<Start>();
        return;
    }
}
