using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.MarketStages;

public class Market(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => base.Message;
    public override IEnumerable<string> Buttons => base.Buttons;
    public override Task HandleMessage(string message)
    {
        return base.HandleMessage(message);
    }
}
