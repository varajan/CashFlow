using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilitiesConfirm(ITermsService termsService, IPersonManager personManager)
    : ConfirmStage(termsService, personManager, 3, "Are you sure want to stop current game?")
{
    public override string Message
    {
        get
        {
            var liability = PersonManager.Read(CurrentUser).Liabilities.First(l => l.MarkedForReduction);
            var reduceLiabilities = Terms.Get(40, CurrentUser, "Reduce Liabilities");
            var type = Terms.Get(-1, CurrentUser, liability.Name.AsString());

            return $"{reduceLiabilities} - {type}. {Yes}?";
        }
    }

    protected override Task OnDismiss()
    {
        var person = PersonManager.Read(CurrentUser);
        person.Liabilities
            .Where(liability => liability.MarkedForReduction)
            .ForEach(liability =>
            {
                liability.MarkedForReduction = false;
                PersonManager.Update(CurrentUser, liability);
            });

        NextStage = New<ReduceLiabilities>();

        return Task.CompletedTask;
    }

    protected override Task OnConfirmed()
    {
        var person = PersonManager.Read(CurrentUser);
        var liability = person.Liabilities.FirstOrDefault(l => l.MarkedForReduction);
        var amount = liability.FullAmount;

        liability.Cashflow = 0;
        liability.FullAmount = 0;
        liability.MarkedForReduction = false;
        liability.Deleted = true;
        person.Cash -= liability.FullAmount;
        
        PersonManager.Update(person);
        PersonManager.Update(CurrentUser, liability);
        PersonManager.AddHistory(ActionType.ReduceLiability, amount, CurrentUser);

        NextStage = PersonManager.Read(CurrentUser).Liabilities.All(l => l.Deleted)
            ? New<Start>()
            : New<ReduceLiabilities>();

        return Task.CompletedTask;
    }
}
