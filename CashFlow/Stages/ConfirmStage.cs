using CashFlow.Interfaces;

namespace CashFlow.Stages;

public abstract class ConfirmStage(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository, int? id = null, string question = null)
    : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => TranslationService.Get(question, CurrentUser);
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

    protected bool IsConfirmed(string message) => MessageEquals(message, "Yes");
    
    protected abstract Task OnConfirmed();

    protected virtual async Task OnDismiss()
    {
        NextStage = New<Start>();
        return;
    }
}
