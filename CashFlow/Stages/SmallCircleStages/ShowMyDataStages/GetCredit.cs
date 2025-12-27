using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class GetCredit(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");
    public override IEnumerable<string> Buttons => ["1000", "2000", "5000", "10 000", "20 000", Cancel];

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var number = message.AsCurrency();
        if (number % 1000 > 0 || number < 1000)
        {
            await CurrentUser.Notify(Terms.Get(24, CurrentUser, "Invalid amount. The amount must be a multiple of 1000"));
            return;
        }

        CurrentUser.GetCredit_OBSOLETE(number);
        NextStage = New<Start>();
    }
}
