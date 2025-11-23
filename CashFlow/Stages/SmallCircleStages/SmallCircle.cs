using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using CashFlow.Interfaces;
using CashFlow.Data.Users.UserData.PersonData;

namespace CashFlow.Stages.SmallCircleStages;

public class SmallCircle(ITermsService termsService, IHistoryManager historyManager, IPersonManager personManager) : BaseStage(termsService)
{
    private IHistoryManager HistoryManager { get; init; } = historyManager;
    private IPersonManager PersonManager { get; init; } = personManager;

    public override string Message => PersonManager.GetDescription(CurrentUser.Id);

    public override List<string> Buttons
    {
        get
        {
            var isHistoryEmpty = HistoryManager.IsEmpty(CurrentUser.Id);
            var isReadyForBigCircle = PersonManager.Read(CurrentUser.Id).ReadyForBigCircle;

            List<string> buttons = isHistoryEmpty
                ? [Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends")]
                : [Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends"), Terms.Get(2, CurrentUser, "History")];

            buttons.AddRange([Terms.Get(81, CurrentUser, "Small Opportunity"), Terms.Get(84, CurrentUser, "Big Opportunity")]);
            buttons.AddRange([Terms.Get(80, CurrentUser, "Downsize"), Terms.Get(39, CurrentUser, "Baby")]);
            buttons.AddRange([Terms.Get(79, CurrentUser, "Pay Check"), Terms.Get(33, CurrentUser, "Give Money")]);

            if (isReadyForBigCircle)
            {
                buttons.Add(Terms.Get(1, CurrentUser, "Go to Big Circle"));
            }

            return buttons;
        }
    }

    public async override Task HandleMessage(string message)
    {
        var isHistoryEmpty = HistoryManager.IsEmpty(CurrentUser.Id);

        CurrentUser.Person_OBSOLETE.Assets.CleanUp();
        if (CurrentUser.Person_OBSOLETE.ReadyForBigCircle)
        {
            var notifyMessage = Terms.Get(68, CurrentUser, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", CurrentUser.Name);
            OtherUsers
                .Where(x => x.Person_OBSOLETE.Exists && x.IsActive)
                .ToList()
                .ForEach(u => u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, 31, "Show my Data"):
                NextStage = New<ShowMyData>();
                return;

            case var m when MessageEquals(m, 140, "Friends"):
                var users = OtherUsers.Where(x => x.IsActive).ToList();
                if (users.Any())
                {
                    NextStage = New<Friends>();
                    return;
                }
                else
                {
                    await CurrentUser.Notify(Terms.Get(141, CurrentUser, "There are no other players."));
                    return;
                }

            case var m when !isHistoryEmpty && MessageEquals(m, 2, "History"):
                NextStage = New<History>();
                return;

            case var m when MessageEquals(m, 81, "Small Opportunity"):
                NextStage = New<SmallOpportunity>();
                return;

            case var m when MessageEquals(m, 84, "Big Opportunity"):
                NextStage = New<BigOpportunity>();
                return;

            case var m when MessageEquals(m, 80, "Downsize"):
                await Downsize();
                return;

            case var m when MessageEquals(m, 39, "Kid"):
                await Kid();
                return;

            case var m when MessageEquals(m, 79, "Pay Check"):
                await GetMoney();
                return;

            case var m when MessageEquals(m, 33, "Give Money"):
                NextStage = New<SendMoney>();
                return;
        }
    }

    private async Task Downsize()
    {
        var expenses = CurrentUser.Person_OBSOLETE.Expenses.Total;
        var info = Terms.Get(87, CurrentUser, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency());
        await CurrentUser.Notify(info);

        if (CurrentUser.Person_OBSOLETE.Cash < expenses)
        {
            var delta = expenses - CurrentUser.Person_OBSOLETE.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            CurrentUser.GetCredit(credit);
            var loanInfo = Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency());
            await CurrentUser.Notify(loanInfo);
        }

        CurrentUser.Person_OBSOLETE.Cash -= expenses;
        CurrentUser.History_OBSOLETE.Add(ActionType.Downsize, expenses);
    }

    private async Task Kid()
    {
        if (CurrentUser.Person_OBSOLETE.Expenses.Children == 3)
        {
            await CurrentUser.Notify(Terms.Get(57, CurrentUser, "You're lucky parent of three children. You don't need one more."));
            return;
        }

        CurrentUser.Person_OBSOLETE.Expenses.Children++;
        CurrentUser.History_OBSOLETE.Add(ActionType.Child, CurrentUser.Person_OBSOLETE.Expenses.Children);

        var termId = CurrentUser.Person_OBSOLETE.Expenses.Children == 1 ? 20 : 25;
        var childrenExpenses = CurrentUser.Person_OBSOLETE.Expenses.ChildrenExpenses.AsCurrency();
        var count = CurrentUser.Person_OBSOLETE.Expenses.Children.ToString();

        await CurrentUser.Notify(Terms.Get(termId, CurrentUser, "{0}, you have {1} children expenses and {2} children.", CurrentUser.Person_OBSOLETE.Profession, childrenExpenses, count));
    }

    private async Task GetMoney()
    {
        var amount = CurrentUser.Person_OBSOLETE.CashFlow;
        CurrentUser.Person_OBSOLETE.Bankruptcy = CurrentUser.Person_OBSOLETE.Cash + amount < 0;

        if (CurrentUser.Person_OBSOLETE.Bankruptcy)
        {
            CurrentUser.History_OBSOLETE.Add(ActionType.Bankruptcy);
            NextStage = New<Bankruptcy>();
        }

        CurrentUser.Person_OBSOLETE.Cash += amount;
        CurrentUser.History_OBSOLETE.Add(ActionType.GetMoney, amount);

        await CurrentUser.Notify(Terms.Get(22, CurrentUser, "Ok, you've got *{0}*", amount.AsCurrency()));
    }
}
