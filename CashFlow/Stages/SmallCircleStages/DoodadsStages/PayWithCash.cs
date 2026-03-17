using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCash(ITermsRepository termsService, IAvailableAssetsRepository availableAssets, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, personManager, userRepository)
{
    protected IAvailableAssetsRepository AvailableAssets { get; } = availableAssets;

    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => AvailableAssets.GetAsCurrency(AssetType.SmallGiveMoney).Append(Cancel);

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
            await CurrentUser.Notify(Terms.Get(150, CurrentUser, "Invalid value. Try again."));
            return;
        }

        var person = PersonService.Read(CurrentUser);

        if (person.Cash < amount)
        {
            var delta = amount - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            person.GetCredit(credit);
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
            await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency()));
        }

        person.Cash -= amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.PayMoney, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
