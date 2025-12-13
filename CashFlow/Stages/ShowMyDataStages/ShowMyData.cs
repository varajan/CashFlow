using CashFlow.Interfaces;

namespace CashFlow.Stages.ShowMyDataStages;

public class ShowMyData(ITermsService termsService) : BaseStage(termsService)
{
}