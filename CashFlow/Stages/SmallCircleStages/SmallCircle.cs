using CashFlow.Data.Consts;
using CashFlow.Extensions;
using CashFlow.Interfaces;
using CashFlow.Stages.BigCircleStages;
using CashFlow.Stages.SmallCircleStages.BigOpportunityStages;
using CashFlow.Stages.SmallCircleStages.DoodadsStages;
using CashFlow.Stages.SmallCircleStages.MarketStages;
using CashFlow.Stages.SmallCircleStages.SendMoneyStages;
using CashFlow.Stages.SmallCircleStages.ShowMyDataStages;
using CashFlow.Stages.SmallCircleStages.SmallOpportunityStages;
using MoreLinq;

namespace CashFlow.Stages.SmallCircleStages;

public class SmallCircle(ITermsRepository termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => PersonService.GetDescription(CurrentUser);

    public override List<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var isHistoryEmpty = PersonService.IsHistoryEmpty(CurrentUser);

            List<string> buttons = isHistoryEmpty
                ? [Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends")]
                : [Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends"), Terms.Get(2, CurrentUser, "History")];

            buttons.AddRange([Terms.Get(81, CurrentUser, "Small Opportunity"), Terms.Get(84, CurrentUser, "Big Opportunity")]);
            buttons.AddRange([Terms.Get(86, CurrentUser, "Doodads"), Terms.Get(85, CurrentUser, "Market")]);
            buttons.AddRange([Terms.Get(80, CurrentUser, "Downsize"), Terms.Get(39, CurrentUser, "Baby")]);
            buttons.AddRange([Terms.Get(79, CurrentUser, "Paycheck"), Terms.Get(33, CurrentUser, "Give Money")]);

            if (person.IsReadyForBigCircle())
            {
                buttons.Add(Terms.Get(1, CurrentUser, "Go to Big Circle"));
            }

            return buttons;
        }
    }

    public async override Task HandleMessage(string message)
    {
        var person = PersonService.Read(CurrentUser);
        var isHistoryEmpty = PersonService.IsHistoryEmpty(CurrentUser);

        if (person.IsReadyForBigCircle())
        {
            var notifyMessage = Terms.Get(68, CurrentUser, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", CurrentUser.Name);
            OtherUsers.Where(x => x.IsActive()).ForEach(u => u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, 31, "Show my Data"):
                NextStage = New<ShowMyData>();
                return;

            case var m when MessageEquals(m, 140, "Friends"):
                var users = OtherUsers.Where(x => x.IsActive()).ToList();
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

            case var m when MessageEquals(m, 86, "Doodads"):
                NextStage = New<Doodads>();
                return;

            case var m when MessageEquals(m, 85, "Market"):
                NextStage = New<Market>();
                return;

            case var m when MessageEquals(m, 39, "Baby"):
                await Baby();
                return;

            case var m when MessageEquals(m, 79, "Paycheck"):
                await GetMoney();
                return;

            case var m when MessageEquals(m, 33, "Give Money"):
                NextStage = New<SendMoney>();
                return;

            case var m when person.IsReadyForBigCircle() && MessageEquals(m, 1, "Go to Big Circle"):
                person.InitialCashFlow = person.Assets.Sum(a => a.CashFlow) / 10 * 1000;
                person.TargetCashFlow = person.InitialCashFlow + 50_000;
                person.Cash += person.InitialCashFlow;
                person.BigCircle = true;

                PersonService.Update(person);
                PersonService.AddHistory(ActionType.GoToBigCircle, person.InitialCashFlow, CurrentUser);
                NextStage = New<BigCircle>();
                return;
        }
    }

    private async Task Downsize()
    {
        var person = PersonService.Read(CurrentUser);
        var expenses = -1 * person.GetTotalExpenses();
        var info = Terms.Get(87, CurrentUser, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency());
        await CurrentUser.Notify(info);

        if (person.Cash < expenses)
        {
            var delta = expenses - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
            var loanInfo = Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency());

            person.GetCredit(credit);
            PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
            await CurrentUser.Notify(loanInfo);
        }

        person.Cash -= expenses;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Downsize, expenses, CurrentUser);
    }

    private async Task Baby()
    {
        var person = PersonService.Read(CurrentUser);

        if (person.Children == 3)
        {
            await CurrentUser.Notify(Terms.Get(57, CurrentUser, "You're lucky parent of three children. You don't need one more."));
            return;
        }

        person.Children++;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Child, person.Children, CurrentUser);

        var termId = person.Children == 1 ? 20 : 25;
        var childrenExpenses = (person.Children * person.PerChild).AsCurrency();
        var count = person.Children.ToString();

        await CurrentUser.Notify(Terms.Get(termId, CurrentUser, "{0}, you have {1} children expenses and {2} children.", person.Profession, childrenExpenses, count));
    }

    private async Task GetMoney()
    {
        var person = PersonService.Read(CurrentUser);
        var amount = person.GetSmallCircleCashflow();

        var bankruptcy = amount < 0 && person.Cash + amount < 0;
        if (bankruptcy)
        {
            await ProcessBankruptcy(person);
            return;
        }

        person.Cash += amount;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.GetMoney, amount, CurrentUser);

        await CurrentUser.Notify(Terms.Get(22, CurrentUser, "Ok, you've got *{0}*", amount.AsCurrency()));
    }
}
