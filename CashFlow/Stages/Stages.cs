using CashFlow.Data;
using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using CashFlow.Data.Users.UserData.PersonData;
using CashFlow.Extensions;

namespace CashFlow.Stages;

public interface IStage
{
    IUser CurrentUser { get; set; }
    string Name { get; }
    string Message { get; }
    IEnumerable<string> Buttons { get; }
    IStage NextStage { get; }
    IStage SetCurrentUser(IUser user);
    IStage SetAllUsers(IList<IUser> users);

    Task HandleMessage(string message);
    Task SetButtons();
}

public abstract class BaseStage : IStage
{
    public string Name => GetType().Name;
    public IUser CurrentUser { get; set; }
    public IList<IUser> OtherUsers { get; set; }
    public virtual string Message => default;
    public virtual IEnumerable<string> Buttons => default;
    public virtual IStage NextStage { get; set; }

    protected ITermsService Terms { get; }

    public BaseStage(ITermsService termsService)
    {
        Terms = termsService;
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
        var @namespace = typeof(BaseStage).Namespace;
        var stage = Type.GetType($"{@namespace}.{currentUser.StageName}");
        if (stage is null) throw new Exception($"{stage} stage not found!");

        var currentStage = (IStage)ServicesProvider.Get(stage);

        return currentStage
            .SetCurrentUser(currentUser)
            .SetAllUsers(otherUsers);
    }

    protected bool MessageEquals(string message, int id, string value) =>
        message.Equals(Terms.Get(id, CurrentUser, value), StringComparison.InvariantCultureIgnoreCase);

    protected IStage New<T>() where T : BaseStage
    {
        var stage = (IStage)ServicesProvider.Get<T>();
        return stage.SetCurrentUser(CurrentUser).SetAllUsers(OtherUsers);
    }

    protected bool IsCanceled(string message)
    {
        if (message.Equals(Cancel, StringComparison.InvariantCultureIgnoreCase))
        {
            NextStage = New<Start>();
            return true;
        }

        return false;
    }

    protected string Cancel => Terms.Get(6, CurrentUser, "Cancel");
}

public class Start(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => NextStage.Message;
    public override IEnumerable<string> Buttons => NextStage.Buttons;

    public override IStage NextStage
    {
        get
        {
            if (CurrentUser.Person_OBSOLETE.Exists)
            {
                return CurrentUser.Person_OBSOLETE.Circle == Circle.Big
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<ChooseProfession>();
        }
    }
}

public class SmallCircle(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => CurrentUser.Person_OBSOLETE.Description;
    public override List<string> Buttons
    {
        get
        {
            List<string> buttons = CurrentUser.History.IsEmpty
                ? [ Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends") ]
                : [Terms.Get(31, CurrentUser, "Show my Data"), Terms.Get(140, CurrentUser, "Friends"), Terms.Get(2, CurrentUser, "History")];

            buttons.AddRange([Terms.Get(81, CurrentUser, "Small Opportunity"), Terms.Get(84, CurrentUser, "Big Opportunity")]);
            buttons.AddRange([Terms.Get(80, CurrentUser, "Downsize"), Terms.Get(39, CurrentUser, "Baby")]);
            buttons.AddRange([Terms.Get(79, CurrentUser, "Pay Check"), Terms.Get(33, CurrentUser, "Give Money")]);

            if (CurrentUser.Person_OBSOLETE.ReadyForBigCircle)
            {
                buttons.Add( Terms.Get(1, CurrentUser, "Go to Big Circle") );
            }

            return buttons;
        }
    }

    public async override Task HandleMessage(string message)
    {
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

            case var m when MessageEquals(m, 2, "History"):
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
        CurrentUser.History.Add(ActionType.Downsize, expenses);
    }

    private async Task Kid()
    {
        if (CurrentUser.Person_OBSOLETE.Expenses.Children == 3)
        {
            await CurrentUser.Notify(Terms.Get(57, CurrentUser, "You're lucky parent of three children. You don't need one more."));
            return;
        }

        CurrentUser.Person_OBSOLETE.Expenses.Children++;
        CurrentUser.History.Add(ActionType.Child, CurrentUser.Person_OBSOLETE.Expenses.Children);

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
            CurrentUser.History.Add(ActionType.Bankruptcy);
            NextStage = New<Bankruptcy>();
        }

        CurrentUser.Person_OBSOLETE.Cash += amount;
        CurrentUser.History.Add(ActionType.GetMoney, amount);

        await CurrentUser.Notify(Terms.Get(22, CurrentUser, "Ok, you've got *{0}*", amount.AsCurrency()));
    }
}

public class ShowMyData(ITermsService termsService) : BaseStage(termsService)
{
}

public class Friends(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message
    {
        get
        {
            var message = string.Empty;
            var onSmall = Terms.Get(142, CurrentUser, "On Small circle:");
            var onBig = Terms.Get(143, CurrentUser, "On Big circle:");

            var onSmallCircle = OtherUsers.Where(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Small).ToList();
            var onBigCircle = OtherUsers.Where(x => x.IsActive && x.Person_OBSOLETE.Circle == Circle.Big).ToList();

            if (onSmallCircle.Any()) message += $"*{onSmall}*\r\n{string.Join("", onSmallCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}\r\n";
            if (onBigCircle.Any()) message += $"*{onBig}* \r\n{string.Join("", onBigCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}";

            return message;
        }
    }

    public override IEnumerable<string> Buttons => OtherUsers.Where(x => x.IsActive).Select(x => x.Name).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var friend = OtherUsers.FirstOrDefault(x => x.Name == message);
        if (friend is null) return;

        await CurrentUser.Notify(friend.Person_OBSOLETE.BigCircle ? friend.Person_OBSOLETE.Description : friend.Description);
        await CurrentUser.Notify(friend.History.TopFive);
    }
}

public class History(ITermsService termsService, IHistoryManager historyManager) : BaseStage(termsService)
{
    private IHistoryManager HistoryManager { get; } = historyManager;

    public override string Message => Records.Any()
        ? string.Join(Environment.NewLine, Records.Select(x => x.Description))
        : Terms.Get(111, CurrentUser, "No records found.");

    public override IEnumerable<string> Buttons => Records.Any() ? [Rollback, Cancel] : [Cancel];

    private List<HistoryDto> Records => HistoryManager.Read(CurrentUser.Id).OrderByDescending(x => x.Id).ToList();
    private string Rollback => Terms.Get(109, CurrentUser, "Rollback last action");

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message))
        {
            NextStage = New<Start>();
            return;
        }

        if (MessageEquals(message, 109, "Rollback last action"))
        {
            HistoryManager.Delete(Records.First().Id);
            //CurrentUser.History.Rollback();
        }

        if (Records.Count == 0)
        {
            await CurrentUser.Notify(Message);
            NextStage = New<Start>();
            return;
        }
    }
}

public class SmallOpportunity(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => Terms.Get(89, CurrentUser, "What do you want?");
    public override IEnumerable<string> Buttons =>
    [
        Terms.Get(35, CurrentUser, "Buy Stocks"),
        Terms.Get(36, CurrentUser, "Sell Stocks"),
        Terms.Get(82, CurrentUser, "Stocks x2"),
        Terms.Get(83, CurrentUser, "Stocks ÷2"),
        Terms.Get(37, CurrentUser, "Buy Real Estate"),
        Terms.Get(94, CurrentUser, "Buy Land"),
        Terms.Get(119, CurrentUser, "Buy coins"),
        Terms.Get(115, CurrentUser, "Start a company"),
        Cancel
    ];

    public override Task HandleMessage(string message)
    {
        CurrentUser.Person_OBSOLETE.Assets.CleanUp();

        switch (message)
        {
            case var m when MessageEquals(m, 35, "Buy Stocks"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 36, "Sell Stocks"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 82, "Stocks x2"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 83, "Stocks ÷2"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 37, "Buy Real Estate"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 94, "Buy Land"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 119, "Buy coins"):
                NextStage = New<BuyCoins>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 115, "Start a company"):
                NextStage = New<StartCompany>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class StartCompany(ITermsService termsService, IAvailableAssets availableAssets) : BaseStage(termsService)
{
    protected IAvailableAssets AvailableAssets { get; } = availableAssets;

    protected Asset_OLD Asset => CurrentUser.Person_OBSOLETE.Assets.SmallBusinesses.First(a => a.IsDraft);

    public override string Message => Terms.Get(7, CurrentUser, "Title:");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessType, CurrentUser.Language).Append(Cancel);

    public override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return Task.CompletedTask;

        CurrentUser.Person_OBSOLETE.Assets.Add(message, AssetType.SmallBusinessType);
        NextStage = New<StartCompanyPrice>();
        return Task.CompletedTask;
    }
}

public class StartCompanyPrice(ITermsService termsService, IAvailableAssets assets) : StartCompany(termsService, assets)
{
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => AvailableAssets.GetAsText(AssetType.SmallBusinessBuyPrice, CurrentUser.Language).Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        if (IsCanceled(message)) return;

        var number = message.AsCurrency();
        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        Asset.Price = number;

        if (CurrentUser.Person_OBSOLETE.Cash < number)
        {
            NextStage = New<StartCompanyCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        CurrentUser.Person_OBSOLETE.Cash -= Asset.Price;
        CurrentUser.History.Add(ActionType.StartCompany, Asset.Id);
        Asset.IsDraft = false;

        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

public class StartCompanyCredit(ITermsService termsService, IAvailableAssets assets) : StartCompanyPrice(termsService, assets)
{
    public override string Message
    {
        get
        {
            var value = Asset.Price.AsCurrency();
            var cash = CurrentUser.Person_OBSOLETE.Cash.AsCurrency();
            return Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", value, cash);
        }
    }

    public override IEnumerable<string> Buttons => [Terms.Get(34, CurrentUser, "Get Credit"), Cancel];

    public override async Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return;

            case var m when MessageEquals(m, 34, "Get Credit"):
                var delta = Asset.Price - CurrentUser.Person_OBSOLETE.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}

// ----------------------------------------

public class BigOpportunity(ITermsService termsService) : BaseStage(termsService)
{
    public override string Message => base.Message;
    public override IEnumerable<string> Buttons => base.Buttons;
    public override Task HandleMessage(string message)
    {
        return base.HandleMessage(message);
    }
}

public class Bankruptcy(ITermsService termsService) : BaseStage(termsService)
{

}

// ------------------------------------------------

public class BigCircle(ITermsService termsService) : BaseStage(termsService)
{
}
