using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;

namespace CashFlow.Stages.BigCircleStages;

public class BigCircle(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService, personManager)
{
}