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

namespace CashFlow.Stages.SmallCircleStages;

public class SmallCircle(ITranslationService termsService, IUserService userService, IPersonService personManager, IUserRepository userRepository)
    : BaseStage(termsService, userService, personManager, userRepository)
{
    public override string Message => PersonService.GetDescription(CurrentUser);

    public override List<string> Buttons
    {
        get
        {
            var person = PersonService.Read(CurrentUser);
            var isHistoryEmpty = PersonService.IsHistoryEmpty(CurrentUser);

            List<string> buttons = isHistoryEmpty
                ? [TranslationService.Get(Terms.ShowData, CurrentUser), TranslationService.Get(Terms.Friends, CurrentUser)]
                : [TranslationService.Get(Terms.ShowData, CurrentUser), TranslationService.Get(Terms.Friends, CurrentUser), TranslationService.Get(Terms.History, CurrentUser)];

            buttons.AddRange([TranslationService.Get(Terms.SmallOpportunity, CurrentUser), TranslationService.Get(Terms.BigOpportunity, CurrentUser)]);
            buttons.AddRange([TranslationService.Get(Terms.Doodads, CurrentUser), TranslationService.Get(Terms.Market, CurrentUser)]);
            buttons.AddRange([TranslationService.Get(Terms.Downsize, CurrentUser), TranslationService.Get(Terms.Baby, CurrentUser)]);
            buttons.AddRange([TranslationService.Get(Terms.Paycheck, CurrentUser), TranslationService.Get(Terms.GiveMoney, CurrentUser)]);
            buttons.AddRange([TranslationService.Get(Terms.GameMenu, CurrentUser)]);

            if (person.IsReadyForBigCircle())
            {
                buttons.Add(TranslationService.Get(Terms.BigCircle, CurrentUser));
            }

            return buttons;
        }
    }

    public async override Task BeforeStage() => await NotifyUserIsReadyForBigCircle();

    public async override Task HandleMessage(string message)
    {
        var person = PersonService.Read(CurrentUser);
        var isHistoryEmpty = PersonService.IsHistoryEmpty(CurrentUser);

        switch (message)
        {
            case var m when MessageEquals(m, Terms.ShowData):
                NextStage = New<ShowMyData>();
                return;

            case var m when MessageEquals(m, Terms.Friends):
                var users = OtherUsers.Where(UserService.IsActive).ToList();
                if (users.Count != 0)
                {
                    NextStage = New<Friends>();
                    return;
                }
                else
                {
                    await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoPlayers, CurrentUser));
                    return;
                }

            case var m when !isHistoryEmpty && MessageEquals(m, Terms.History):
                NextStage = New<History>();
                return;

            case var m when MessageEquals(m, Terms.SmallOpportunity):
                NextStage = New<SmallOpportunity>();
                return;

            case var m when MessageEquals(m, Terms.BigOpportunity):
                NextStage = New<BigOpportunity>();
                return;

            case var m when MessageEquals(m, Terms.Downsize):
                await Downsize();
                return;

            case var m when MessageEquals(m, Terms.Doodads):
                NextStage = New<Doodads>();
                return;

            case var m when MessageEquals(m, Terms.Market):
                NextStage = New<Market>();
                return;

            case var m when MessageEquals(m, Terms.Baby):
                await Baby();
                return;

            case var m when MessageEquals(m, Terms.Paycheck):
                await GetMoney();
                return;

            case var m when MessageEquals(m, Terms.GiveMoney):
                NextStage = New<SendMoney>();
                return;

            case var m when MessageEquals(m, Terms.GameMenu):
                NextStage = New<GameMenu>();
                return;

            case var m when person.IsReadyForBigCircle() && MessageEquals(m, Terms.BigCircle):
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
        var info = TranslationService.Get(Terms.Fired, CurrentUser, expenses.AsCurrency());
        await UserService.Notify(CurrentUser, info);

        if (person.Cash < expenses)
        {
            var delta = expenses - person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
            var loanInfo = TranslationService.Get(Terms.TookLoan, CurrentUser, credit.AsCurrency());

            person.GetCredit(credit);
            PersonService.AddHistory(ActionType.Credit, credit, CurrentUser);
            await UserService.Notify(CurrentUser, loanInfo);
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
            await UserService.Notify(CurrentUser, TranslationService.Get(Terms.NoMoreKids, CurrentUser));
            return;
        }

        person.Children++;
        PersonService.Update(person);
        PersonService.AddHistory(ActionType.Child, person.Children, CurrentUser);

        var termKey = person.Children == 1 ? Terms.ChildExpense1 : Terms.ChildExpenseMany;
        var childrenExpenses = (person.Children * person.PerChild).AsCurrency();
        var count = person.Children.ToString();
        var personProfession = TranslationService.Get(person.Profession, CurrentUser.Language);

        await UserService.Notify(CurrentUser, TranslationService.Get(termKey, CurrentUser, personProfession, childrenExpenses, count));
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

        await UserService.Notify(CurrentUser, TranslationService.Get(Terms.GotAmount, CurrentUser, amount.AsCurrency()));
    }
}
