using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilitiesConfirm(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    : ConfirmStage(termsService, personManager, userRepository, 3, "Are you sure want to stop current game?")
{
    public override string Message
    {
        get
        {
            var liability = PersonService.Read(CurrentUser).Liabilities.First(l => l.MarkedForReduction);
            var reduceLiabilities = Terms.Get(40, CurrentUser, "Reduce Liabilities");
            var type = Terms.Get((int)liability.Type, CurrentUser, liability.Type.AsString());

            return $"{reduceLiabilities} - {type}. {Yes}?";
        }
    }

    protected override Task OnDismiss()
    {
        var person = PersonService.Read(CurrentUser);
        person.Liabilities
            .Where(liability => liability.MarkedForReduction)
            .ForEach(liability =>
            {
                liability.MarkedForReduction = false;
                PersonService.Update(CurrentUser, liability);
            });

        NextStage = New<ReduceLiabilities>();

        return Task.CompletedTask;
    }

    protected override Task OnConfirmed()
    {
        var person = PersonService.Read(CurrentUser);
        var liability = person.Liabilities.FirstOrDefault(l => l.MarkedForReduction);
        var amount = liability.FullAmount;

        if (liability.Type == Liability.Boat_Loan)
        {
            var boat = person.Assets.FirstOrDefault(a => a.Type == AssetType.Boat);
            boat.CashFlow = 0;
            PersonService.UpdateAsset(CurrentUser, boat);
        }

        liability.Cashflow = 0;
        liability.FullAmount = 0;
        liability.MarkedForReduction = false;
        liability.Deleted = true;
        person.Cash -= amount;
        
        PersonService.Update(person);
        PersonService.Update(CurrentUser, liability);
        PersonService.AddHistory((ActionType)liability.Type, amount, CurrentUser);

        NextStage = PersonService.Read(CurrentUser).Liabilities.All(l => l.FullAmount == 0)
            ? New<Start>()
            : New<ReduceLiabilities>();

        return Task.CompletedTask;
    }
}
