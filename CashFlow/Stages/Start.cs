using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Data;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;

namespace CashFlow.Stages;

public class Start(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => NextStage.Message;
    public override IEnumerable<string> Buttons => NextStage.Buttons;

    public override IStage NextStage
    {
        get
        {
            if (CurrentUser.Person_OBSOLETE.Exists)
            {
                return CurrentUser.Person_OBSOLETE.Circle == Circle.Big
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<ChooseProfession>();
        }
    }
}
