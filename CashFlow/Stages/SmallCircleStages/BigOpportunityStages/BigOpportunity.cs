using CashFlow.Data;

namespace CashFlow.Stages.SmallCircleStages.BigOpportunityStages;

public class BigOpportunity(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => base.Message;
    public override IEnumerable<string> Buttons => base.Buttons;
    public override Task HandleMessage(string message)
    {
        return base.HandleMessage(message);
    }
}
