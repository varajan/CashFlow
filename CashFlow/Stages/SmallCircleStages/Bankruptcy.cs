using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages;

public class Bankruptcy(ITermsService termsService) : BaseStage(termsService)
{
}