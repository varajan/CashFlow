using CashFlow.Data.Consts;
using CashFlow.Data.Consts.Terms;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCreditCard(ITranslationService termsService, IUserService userService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => TranslationService.Get(Terms.AskHowMany, CurrentUser);

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.MicroCreditAmount).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        var amount = message.AsCurrency();
        if (amount <= 0)
        {
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.InvalidValue, CurrentUser));
            return;
        }

        var person = PersonService.Read(CurrentUser);
        person.UpdateLiability(Liability.CreditCard, -(int)(amount * 0.03), amount);
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.MicroCredit, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
