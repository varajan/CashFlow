using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Interfaces;

namespace CashFlow.Stages;

public class Start(ITermsService termsService, IPersonManager personManager) : BaseStage(termsService)
{
    private IPersonManager PersonManager { get; } = personManager;

    public override string Message => NextStage.Message;
    public override IEnumerable<string> Buttons => NextStage.Buttons;

    public override IStage NextStage
    {
        get
        {
            if (PersonManager.Exists(CurrentUser.Id))
            {
                var isBigCircle = PersonManager.Read(CurrentUser.Id).BigCircle;

                return isBigCircle
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<ChooseProfession>();
        }
    }
}
