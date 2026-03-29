using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.SmallCircleStages.BankruptcyStages;

namespace CashFlow.Stages;

public abstract class BaseStage : IStage
{
    public string Name => GetType().FullName;
    public UserDto CurrentUser { get; private set; }
    public virtual string Message => default;
    public virtual IEnumerable<string> Buttons => default;
    public virtual IStage NextStage { get; set; }
    protected ITermsRepository Terms { get; }
    protected IPersonService PersonService { get; }
    protected IUserRepository UserRepository { get; }

    public IList<UserDto> OtherUsers => [.. UserRepository.GetAll().Where(u => u.Id != CurrentUser.Id)];

    public BaseStage(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository)
    {
        Terms = termsService;
        PersonService = personManager;
        UserRepository = userRepository;
        NextStage = this;
    }

    public IStage SetCurrentUser(UserDto user)
    {
        CurrentUser = user;
        CurrentUser.StageName = Name;
        UserRepository.Save(CurrentUser);
        return this;
    }

    public virtual Task BeforeStage() { return Task.CompletedTask; }
    public virtual Task HandleMessage(string message) { return Task.CompletedTask; }
    public Task SetButtons() => CurrentUser.SetButtons(this);

    public static IStage GetCurrentStage(UserDto currentUser)
    {
        var stage = Type.GetType(currentUser.StageName);
        if (stage is null) throw new Exception($"<{stage}> stage not found!");

        var currentStage = (IStage)ServicesProvider.Get(stage);

        return currentStage.SetCurrentUser(currentUser);
    }

    protected IStage New<T>() where T : BaseStage
    {
        var stage = (IStage)ServicesProvider.Get<T>();
        return stage.SetCurrentUser(CurrentUser);
    }

    protected async Task ProcessBankruptcy(PersonDto person)
    {
        if (person.Bankruptcy == false)
        {
            person.Bankruptcy = true;
            PersonService.Update(person);
            PersonService.AddHistory(ActionType.Bankruptcy, 0, CurrentUser);
        }

        if (person.Assets.Any(a => !a.IsDeleted))
        {
            NextStage = New<BankruptcySellAssets>();
            return;
        }

        await DebtRestructuring(person);

        var isCashFlowPositive = person.GetSmallCircleCashflow() >= 0;
        if (isCashFlowPositive)
        {
            await CurrentUser.Notify(Terms.Get(130, CurrentUser,
                "You have paid off your debts and can continue, but you must skip your next three turns."));

            person.Bankruptcy = false;
            PersonService.Update(person);
            NextStage = New<Start>();
            return;
        }

        NextStage = New<Bankruptcy>();
    }

    private async Task DebtRestructuring(PersonDto person)
    {
        var isCashFlowPositive = person.GetSmallCircleCashflow() >= 0;
        if (isCashFlowPositive) return;

        foreach (var liability in person.Liabilities.Where(l => l.IsBankruptcyDivisible))
        {
            liability.FullAmount /= 2;
            liability.Cashflow /= 2;
            PersonService.Update(CurrentUser, liability);
        }
        PersonService.Update(person);
        await CurrentUser.Notify(Terms.Get(134, CurrentUser, "Debt restructuring. Car loans, small loans and credit card halved."));
        PersonService.AddHistory(ActionType.BankruptcyDebtRestructuring, 0, CurrentUser);
    }

    protected bool MessageEquals(string message, int id, string value) =>
        message.Equals(Terms.Get(id, CurrentUser, value), StringComparison.InvariantCultureIgnoreCase);

    protected bool IsCanceled(string message) => MessageEquals(message, 6, "Cancel");

    protected string Yes => Terms.Get(4, CurrentUser, "Yes");
    protected string No => Terms.Get(138, CurrentUser, "No");
    protected string Cancel => Terms.Get(6, CurrentUser, "Cancel");
    protected string GetCredit => Terms.Get(34, CurrentUser, "Get Credit");
    protected string StopGame => Terms.Get(41, CurrentUser, "Stop Game");
    protected string History => Terms.Get(2, CurrentUser, "History");
}
