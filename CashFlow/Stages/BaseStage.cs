using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages;

namespace CashFlow.Stages;

public abstract class BaseStage : IStage
{
    public string Name => GetType().FullName;
    public IUser CurrentUser { get; private set; }
    public IList<IUser> OtherUsers { get; set; }
    public virtual string Message => default;
    public virtual IEnumerable<string> Buttons => default;
    public virtual IStage NextStage { get; set; }
    protected ITermsService Terms { get; }
    protected IPersonManager PersonManager { get; }

    public BaseStage(ITermsService termsService, IPersonManager personManager)
    {
        Terms = termsService;
        PersonManager = personManager;
        NextStage = this;
    }

    public IStage SetCurrentUser(IUser user)
    {
        CurrentUser = user;
        CurrentUser.StageName = Name;
        return this;
    }

    public IStage SetAllUsers(IList<IUser> users)
    {
        OtherUsers = users;
        return this;
    }

    public virtual Task HandleMessage(string message) { return Task.CompletedTask; }
    public Task SetButtons() => CurrentUser.SetButtons(this);

    public static IStage GetCurrentStage(IList<IUser> otherUsers, IUser currentUser)
    {
        var stage = Type.GetType(currentUser.StageName);
        if (stage is null) throw new Exception($"<{stage}> stage not found!");

        var currentStage = (IStage)ServicesProvider.Get(stage);

        return currentStage
            .SetCurrentUser(currentUser)
            .SetAllUsers(otherUsers);
    }

    protected IStage New<T>() where T : BaseStage
    {
        var stage = (IStage)ServicesProvider.Get<T>();
        return stage.SetCurrentUser(CurrentUser).SetAllUsers(OtherUsers);
    }

    protected async Task<bool> IsBankruptcy(PersonDto person, int amount)
    {
        var bankruptcy = amount < 0 && person.Cash + amount < 0;
        if (!bankruptcy) return false;

        if (person.Assets.Any(a => !a.IsDeleted))
        {
            NextStage = New<BankruptcySellAssets>();
            return true;
        }

        // divide Car Loan, Credit cards, retail debt by 2
        foreach (var liability in person.Liabilities.Where(l => l.IsBankruptcyDivisible))
        {
            liability.FullAmount /= 2;
            liability.Cashflow /= 2;
            PersonManager.UpdateLiability(person.Id, liability);
        }
        await CurrentUser.Notify(Terms.Get(134, CurrentUser, "Debt restructuring. Car loans, small loans and credit card halved."));

        // person = PersonManager.Read(CurrentUser.Id);
        if (person.CashFlow > 0)
        {
            await CurrentUser.Notify(Terms.Get(130, CurrentUser, "You have paid off your debts, you can continue."));
            // LOSE 3 turns!
            return false;
        }

        PersonManager.AddHistory(ActionType.Bankruptcy, 0, CurrentUser);
        NextStage = New<Bankruptcy>();
        return true;
    }

    protected bool MessageEquals(string message, int id, string value) =>
    message.Equals(Terms.Get(id, CurrentUser, value), StringComparison.InvariantCultureIgnoreCase);

    protected bool IsCanceled(string message) => MessageEquals(message, 6, "Cancel");

    protected string Yes => Terms.Get(4, CurrentUser, "Yes");
    protected string No => Terms.Get(138, CurrentUser, "No");
    protected string Cancel => Terms.Get(6, CurrentUser, "Cancel");
    protected string GetCredit => Terms.Get(34, CurrentUser, "Get Credit");
}
