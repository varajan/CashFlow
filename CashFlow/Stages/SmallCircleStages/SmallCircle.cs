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

public class SmallCircle(ITranslationService termsService, IPersonService personManager, IUserRepository userRepository) : BaseStage(termsService, personManager, userRepository)
{
    public override string Message => PersonService.GetDescription(CurrentUser);

    public override List<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var isHistoryEmpty = PersonService.IsHistoryEmpty(CurrentUser);

            List<string> buttons = isHistoryEmpty
                ? [Terms.Get("Show my Data", CurrentUser), Terms.Get("Friends", CurrentUser)]
                : [Terms.Get("Show my Data", CurrentUser), Terms.Get("Friends", CurrentUser), Terms.Get("History", CurrentUser)];

            buttons.AddRange([Terms.Get("Small Opportunity", CurrentUser), Terms.Get("Big Opportunity", CurrentUser)]);
            buttons.AddRange([Terms.Get("Doodads", CurrentUser), Terms.Get("Market", CurrentUser)]);
            buttons.AddRange([Terms.Get("Downsize", CurrentUser), Terms.Get("Baby", CurrentUser)]);
            buttons.AddRange([Terms.Get("Paycheck", CurrentUser), Terms.Get("Give Money", CurrentUser)]);

            if (person.IsReadyForBigCircle())
            {
                buttons.Add(Terms.Get("Go to Big Circle", CurrentUser));
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
            var notifyMessage = Terms.Get("{0}'s income is greater, then expenses. {0} is ready for Big Circle.", CurrentUser, CurrentUser.Name);
            OtherUsers.Where(x => x.IsActive()).ForEach(async u => await u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, "Show my Data"):
                NextStage = New<ShowMyData>();
                return;

            case var m when MessageEquals(m, "Friends"):
                var users = OtherUsers.Where(x => x.IsActive()).ToList();
                if (users.Count != 0)
                {
                    NextStage = New<Friends>();
                    return;
                }
                else
                {
                    await CurrentUser.Notify(Terms.Get("There are no other players.", CurrentUser));
                    return;
                }

            case var m when !isHistoryEmpty && MessageEquals(m, "History"):
                NextStage = New<History>();
                return;

            case var m when MessageEquals(m, "Small Opportunity"):
                NextStage = New<SmallOpportunity>();
                return;

            case var m when MessageEquals(m, "Big Opportunity"):
                NextStage = New<BigOpportunity>();
                return;

            case var m when MessageEquals(m, "Downsize"):
                await Downsize();
                return;

            case var m when MessageEquals(m, "Doodads"):
                NextStage = New<Doodads>();
                return;

            case var m when MessageEquals(m, "Market"):
                NextStage = New<Market>();
                return;

            case var m when MessageEquals(m, "Baby"):
                await Baby();
                return;

            case var m when MessageEquals(m, "Paycheck"):
                await GetMoney();
                return;

            case var m when MessageEquals(m, "Give Money"):
                NextStage = New<SendMoney>();
                return;

            case var m when person.IsReadyForBigCircle() && MessageEquals(m, "Go to Big Circle"):
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
        var info = Terms.Get("You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", CurrentUser, expenses.AsCurrency());
        await CurrentUser.Notify(info);

        if (person.Cash < expenses)
        {
            var delta = expenses - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
            var loanInfo = Terms.Get("You've taken {0} from bank.", CurrentUser, credit.AsCurrency());

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
            await CurrentUser.Notify(Terms.Get("You're lucky parent of three children. You don't need one more.", CurrentUser));
            return;
        }

        person.Children++;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Child, person.Children, CurrentUser);

        var termKey = person.Children == 1
            ? "{0}, you have {1} children expenses and {2} children."
            : "{0}, you have {1} children expenses and {2} children.";
        var childrenExpenses = (person.Children * person.PerChild).AsCurrency();
        var count = person.Children.ToString();
        var personProfession = Terms.Get(person.Profession, CurrentUser.Language);

        await CurrentUser.Notify(Terms.Get(termKey, CurrentUser, personProfession, childrenExpenses, count));
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

        await CurrentUser.Notify(Terms.Get("Ok, you've got *{0}*", CurrentUser, amount.AsCurrency()));
    }
}
