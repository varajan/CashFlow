using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages;

public class ShowMyData(ITermsService termsService) : BaseStage(termsService)
{
}