using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class Start(ITermsRepository termsService, IPersonService personManager) : BaseStage(termsService, personManager)
{
    public override string Message => NextStage.Message;
    public override IEnumerable<string> Buttons => NextStage.Buttons;
    public override Task BeforeStage() => NextStage.BeforeStage();

    public override IStage NextStage
    {
        get
        {
            if (PersonManager.Exists(CurrentUser))
            {
                var isBigCircle = PersonManager.Read(CurrentUser).BigCircle;

                return isBigCircle
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<ChooseProfession>();
        }
    }
}
