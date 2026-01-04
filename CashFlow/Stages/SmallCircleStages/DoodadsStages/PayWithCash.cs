using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;

namespace CashFlow.Stages.SmallCircleStages.DoodadsStages;

public class PayWithCash(ITermsService termsService, IAvailableAssets availableAssets, IPersonManager personManager) : BaseStage(termsService, personManager)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;

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

        var person = PersonManager.Read(CurrentUser);

        if (person.Cash < amount)
        {
            var delta = amount - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            person.GetCredit(credit);
            PersonManager.Update(person);
            PersonManager.AddHistory(ActionType.Credit, credit, CurrentUser);
            await CurrentUser.Notify(Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency()));
        }

        person.Cash -= amount;
        PersonManager.Update(person);
        PersonManager.AddHistory(ActionType.PayMoney, amount, CurrentUser);
        NextStage = New<Start>();
    }
}
