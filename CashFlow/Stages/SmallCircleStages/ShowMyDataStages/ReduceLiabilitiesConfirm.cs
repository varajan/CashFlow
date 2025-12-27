using CashFlow.Data.Consts;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages.ShowMyDataStages;

public class ReduceLiabilitiesConfirm(ITermsService termsService, IPersonManager personManager)
    : ConfirmStage(termsService, 3, "Are you sure want to stop current game?")
{
    public override string Message
    {
        get
        {
            var liability = PersonManager.Read(CurrentUser.Id).Liabilities.First(l => l.MarkedForReduction);
            var reduceLiabilities = Terms.Get(40, CurrentUser, "Reduce Liabilities");
            var type = Terms.Get(-1, CurrentUser, liability.Name);

            return $"{reduceLiabilities} - {type}. {Yes}?";
        }
    }

    protected IPersonManager PersonManager { get; } = personManager;

    protected override Task OnDismiss()
    {
        var person = PersonManager.Read(CurrentUser.Id);
        person.Liabilities
            .Where(liability => liability.MarkedForReduction)
            .ForEach(liability =>
            {
                liability.MarkedForReduction = false;
                PersonManager.UpdateLiability(CurrentUser.Id, liability);
            });

        NextStage = New<ReduceLiabilities>();

        return Task.CompletedTask;
    }

    protected override Task OnConfirmed()
    {
        var person = PersonManager.Read(CurrentUser.Id);
        var liability = person.Liabilities.FirstOrDefault(l => l.MarkedForReduction);
        var amount = liability.FullAmount;

        liability.Cashflow = 0;
        liability.FullAmount = 0;
        liability.MarkedForReduction = false;
        liability.Deleted = true;
        person.Cash -= liability.FullAmount;
        
        PersonManager.Update(person);
        PersonManager.UpdateLiability(CurrentUser.Id, liability);
        PersonManager.AddHistory(ActionType.ReduceLiability, amount, CurrentUser);

        NextStage = PersonManager.Read(CurrentUser.Id).Liabilities.All(l => l.Deleted)
            ? New<Start>()
            : New<ReduceLiabilities>();

        return Task.CompletedTask;
    }
}
