using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public abstract class ConfirmStage(ITermsService termsService, IPersonManager personManager, int? id = null, string question = null)
    : BaseStage(termsService, personManager)
{
    public override string Message => Terms.Get(id.Value, CurrentUser, question);
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
