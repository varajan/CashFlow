using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class GetMoney(ITermsService termsService) : BaseStage(termsService)
{
}

public class GetCredit(ITermsService termsService) : BaseStage(termsService)
{
}

public class ReduceLiabilities(ITermsService termsService) : BaseStage(termsService)
{
}
