using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;

namespace CashFlow.Stages;

public class Start(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => NextStage.Message;
    public override IEnumerable<string> Buttons => NextStage.Buttons;
    public override Task BeforeStage() => NextStage.BeforeStage();

    public override IStage NextStage
    {
        get
        {
            if (PersonService.Exists(CurrentUser))
            {
                var person = PersonService.Read(CurrentUser);

                if (person.BigCircle)
                    return New<BigCircle>();

                if (person.Bankruptcy && person.Assets.Any(a => !a.IsDeleted))
                    return New<BankruptcySellAssets>();

                if (person.Bankruptcy)
                    return New<Bankruptcy>();

                return New<SmallCircle>();
            }

            return New<ChooseProfession>();
        }
    }
}
