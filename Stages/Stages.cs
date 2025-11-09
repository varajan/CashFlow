using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users;
using CashFlowBot.Data.Users.UserData.PersonData;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Fabric;

namespace CashFlowBot.Stages;

public interface IStage
{
    IUser CurrentUser { get; init; }
    string Name { get; }
    string Message { get; }
    IEnumerable<string> Buttons { get; }
    IStage NextStage { get; }

    Task HandleMessage(string message);
    Task SetButtons();
}

public abstract class BaseStage : IStage
{
    public string Name => GetType().Name;
    public IUser CurrentUser { get; init; }
    public IList<IUser> OtherUsers { get; init; }
    public virtual string Message => default;
    public virtual IEnumerable<string> Buttons => default;
    public virtual IStage NextStage { get; set; }

    protected IAvailableAssets Assets { get; }
    protected ITermsService Terms { get; }
    protected ILogger Logger { get; }

    public BaseStage(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets)
    {
        Terms = termsService;
        CurrentUser = currentUser;
        OtherUsers = otherUsers;
        Logger = logger;
        Assets = assets;
        CurrentUser.StageName = Name;
        NextStage = this;
    }

    public virtual Task HandleMessage(string message) { return Task.CompletedTask; }
    public Task SetButtons() => CurrentUser.SetButtons(this);

    public static IStage GetCurrentStage(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets)
    {
        var stage = Type.GetType($"CashFlowBot.Stages.{currentUser.StageName}");
        return stage is not null
            ? (IStage)Activator.CreateInstance(stage, otherUsers, currentUser, termsService, logger, assets)
            : throw new Exception($"{stage} stage not found!");
    }

    protected bool MessageEquals(string message, int id, string value) =>
        message.Equals(Terms.Get(id, CurrentUser, value), StringComparison.InvariantCultureIgnoreCase);

    protected IStage New<T>() where T : BaseStage
    {
        var stage = Type.GetType($"CashFlowBot.Stages.{nameof(T)}");
        return stage is not null
            ? (IStage)Activator.CreateInstance(stage, OtherUsers, CurrentUser, Terms, Logger, Assets)
            : throw new Exception($"{stage} stage not found!");
    }

    protected string Cancel => Terms.Get(6, CurrentUser, "Cancel");
}

public class Start(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override IStage NextStage
    {
        get
        {
            if (CurrentUser.Person.Exists)
            {
                return CurrentUser.Person.Circle == Circle.Big
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<ChooseLanguage>();
        }
    }
}

public class SmallCircle(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => CurrentUser.Person.Description;
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

            if (CurrentUser.Person.ReadyForBigCircle)
            {
                buttons.Add( Terms.Get(1, CurrentUser, "Go to Big Circle") );
            }

            return buttons;
        }
    }

    public override IStage NextStage => New<SmallCircle>();

    public async override Task HandleMessage(string message)
    {
        CurrentUser.Person.Assets.CleanUp();
        if (CurrentUser.Person.ReadyForBigCircle)
        {
            var notifyMessage = Terms.Get(68, CurrentUser, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", CurrentUser.Name);
            OtherUsers
                .Where(x => x.Person.Exists && x.IsActive)
                .ForEach(u => u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, 31, "Show my Data"):
                NextStage = New<ShowMyData>();
                return;

            case var m when MessageEquals(m, 141, "Friends"):
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
                NextStage = New<ShowHistory>();
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
        var expenses = CurrentUser.Person.Expenses.Total;
        var info = Terms.Get(87, CurrentUser, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency());
        await CurrentUser.Notify(info);

        if (CurrentUser.Person.Cash < expenses)
        {
            var delta = expenses - CurrentUser.Person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            CurrentUser.GetCredit(credit);
            var loanInfo = Terms.Get(88, CurrentUser, "You've taken {0} from bank.", credit.AsCurrency());
            await CurrentUser.Notify(loanInfo);
        }

        CurrentUser.Person.Cash -= expenses;
        CurrentUser.History.Add(ActionType.Downsize, expenses);
    }

    private async Task Kid()
    {
        if (CurrentUser.Person.Expenses.Children == 3)
        {
            await CurrentUser.Notify(Terms.Get(57, CurrentUser, "You're lucky parent of three children. You don't need one more."));
            return;
        }

        CurrentUser.Person.Expenses.Children++;
        CurrentUser.History.Add(ActionType.Child, CurrentUser.Person.Expenses.Children);

        var termId = CurrentUser.Person.Expenses.Children == 1 ? 20 : 25;
        var childrenExpenses = CurrentUser.Person.Expenses.ChildrenExpenses.AsCurrency();
        var count = CurrentUser.Person.Expenses.Children.ToString();

        await CurrentUser.Notify(Terms.Get(termId, CurrentUser, "{0}, you have {1} children expenses and {2} children.", CurrentUser.Person.Profession, childrenExpenses, count));
    }

    private async Task GetMoney()
    {
        var amount = CurrentUser.Person.CashFlow;
        CurrentUser.Person.Bankruptcy = CurrentUser.Person.Cash + amount < 0;

        if (CurrentUser.Person.Bankruptcy)
        {
            CurrentUser.History.Add(ActionType.Bankruptcy);
            NextStage = New<Bankruptcy>();
        }

        CurrentUser.Person.Cash += amount;
        CurrentUser.History.Add(ActionType.GetMoney, amount);

        await CurrentUser.Notify(Terms.Get(22, CurrentUser, "Ok, you've got *{0}*", amount.AsCurrency()));
    }
}

public class ShowMyData(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
}

public class Friends(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message
    {
        get
        {
            var message = string.Empty;
            var onSmall = Terms.Get(142, CurrentUser, "On Small circle:");
            var onBig = Terms.Get(143, CurrentUser, "On Big circle:");

            var onSmallCircle = OtherUsers.Where(x => x.IsActive && x.Person.Circle == Circle.Small).ToList();
            var onBigCircle = OtherUsers.Where(x => x.IsActive && x.Person.Circle == Circle.Big).ToList();

            if (onSmallCircle.Any()) message += $"*{onSmall}*\r\n{string.Join("", onSmallCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}\r\n";
            if (onBigCircle.Any()) message += $"*{onBig}* \r\n{string.Join("", onBigCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}";

            return message;
        }
    }

    public override List<string> Buttons => OtherUsers.Where(x => x.IsActive).Select(x => x.Name).Append(Cancel).ToList();

    public async override Task HandleMessage(string message)
    {
        var friend = OtherUsers.FirstOrDefault(x => x.Name == message);
        if (friend is null) return;

        await CurrentUser.Notify(friend.Person.BigCircle ? friend.Person.Description : friend.Description);
        await CurrentUser.Notify(friend.History.TopFive);
    }
}

public class ShowHistory(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => CurrentUser.History.Description;
    public override IEnumerable<string> Buttons => CurrentUser.History.IsEmpty ? [Cancel] : [Rollback, Cancel];

    private string Rollback => Terms.Get(109, CurrentUser, "Rollback last action");

    public override Task HandleMessage(string message)
    {
        switch (message)
        {
            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 6, "Rollback last action"):
                CurrentUser.History.Rollback();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class SmallOpportunity(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
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
        CurrentUser.Person.Assets.CleanUp();

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
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 115, "Start a company"):
                //NextStage = New<Start>();
                return Task.CompletedTask;

            case var m when MessageEquals(m, 6, "Cancel"):
                NextStage = New<Start>();
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class BuyCoins(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(7, CurrentUser, "Title:");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var cancel = Cancel;
            var coins = Assets.GetAsText(AssetType.CoinTitle, CurrentUser.Language).Append(cancel);

            return coins;
        }
    }

    public override Task HandleMessage(string message)
    {
        var coinTitle = Assets
            .GetAsText(AssetType.CoinTitle, CurrentUser.Language)
            .FirstOrDefault(x => x.Equals(message, StringComparison.InvariantCultureIgnoreCase));

        if (coinTitle is not null)
        {
            CurrentUser.Person.Assets.Add(coinTitle, AssetType.Coin);
            NextStage = New<BuyCoinsCount>();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}


public class BuyCoinsCount(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons => Assets
        .GetAsText(AssetType.CoinCount, CurrentUser.Language)
        .Append(Cancel);

    public async override Task HandleMessage(string message)
    {
        var number = message.AsCurrency();

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(18, CurrentUser, "Invalid quantity value. Try again."));
            return;
        }

        CurrentUser.Person.Assets.Coins.First(a => a.IsDraft).Qtty = number;
        NextStage = New<BuyCoinsPrice>();
    }
}

public class BuyCoinsPrice(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    protected Asset Asset => CurrentUser.Person.Assets.Coins.First(a => a.IsDraft);
    public override string Message => Terms.Get(8, CurrentUser, "What is the price?");
    public override IEnumerable<string> Buttons => Assets.GetAsCurrency(AssetType.CoinBuyPrice).Append(Cancel);

    public override async Task HandleMessage(string message)
    {
        var number = message.AsCurrency();
        var totalPrice = Asset.Price * Asset.Qtty;

        if (number <= 0)
        {
            await CurrentUser.Notify(Terms.Get(9, CurrentUser, "Invalid price value. Try again."));
            return;
        }

        Asset.Price = number;

        if (CurrentUser.Person.Cash < totalPrice)
        {
            await CurrentUser.Notify(Terms.Get(23, CurrentUser, "You don''t have {0}, but only {1}", totalPrice.AsCurrency(), CurrentUser.Person.Cash.AsCurrency()));

            NextStage = New<BuyCoinsCredit>();
            return;
        }

        await CompleteTransaction();
        NextStage = New<Start>();
    }

    protected async Task CompleteTransaction()
    {
        Asset.IsDraft = false;
        CurrentUser.Person.Cash -= Asset.Price * Asset.Qtty;
        CurrentUser.History.Add(ActionType.BuyCoins, Asset.Id);
        await CurrentUser.Notify(Terms.Get(13, CurrentUser, "Done."));
    }
}

public class BuyCoinsCredit(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BuyCoinsPrice(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message
    {
        get
        {
            var value = CurrentUser.Person.Assets.Transfer.Qtty.AsCurrency();
            var cash = CurrentUser.Person.Cash.AsCurrency();
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
                var delta = Asset.Price * Asset.Qtty - CurrentUser.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

                CurrentUser.GetCredit(credit);
                await CompleteTransaction();

                NextStage = New<Start>();
                return;
        }
    }
}
// ----------------------------------------

public class BigOpportunity(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
}

public class SendMoney(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(147, CurrentUser, "Whom?");

    public override List<string> Buttons
    {
        get
        {
            CurrentUser.Person.Assets.Transfer?.Delete();

            var cancel = Cancel;
            var bank = Terms.Get(149, CurrentUser, "Bank");
            var users = OtherUsers.Where(x => x.IsActive && x.Person.Circle == Circle.Small).Select(x => x.Name).ToList();

            return users.Append(bank).Append(cancel).ToList();
        }
    }

    public async override Task HandleMessage(string message)
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");

        if (message == bank || OtherUsers.Any(x => x.IsActive && x.Person.Circle == Circle.Small && x.Name == message))
        {
            CurrentUser.Person.Assets.Add(message, AssetType.Transfer);
            NextStage = New<SendMoneyTo>();
            return;
        }

        await CurrentUser.Notify(Terms.Get(145, CurrentUser, "Not found."));
    }
}

public class SendMoneyTo(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(21, CurrentUser, "How much?");

    public override IEnumerable<string> Buttons
    {
        get
        {
            var cancel = Cancel;
            return Enumerable.Range(1, 8)
                .Select(x => (500 * x).AsCurrency())
                .Append(cancel);
        }
    }

    public override async Task HandleMessage(string message)
    {
        CurrentUser.Person.Assets.Transfer.Qtty = message.AsCurrency();
        if (CurrentUser.Person.Cash <= CurrentUser.Person.Assets.Transfer.Qtty)
        {
            NextStage = New<SendMoneyToWithCredit>();
            return;
        }

        await Transfer();
    }

    protected async Task Transfer()
    {
        var bank = Terms.Get(149, CurrentUser, "Bank");
        var to = CurrentUser.Person.Assets.Transfer.Title;
        var amount = CurrentUser.Person.Assets.Transfer.Qtty;
        var friend = OtherUsers.FirstOrDefault(x => x.Name == to);
        var message = Terms.Get(146, CurrentUser, "{0} transferred {2} to {1}.", CurrentUser.Name, friend?.Name ?? bank, amount.AsCurrency(), Environment.NewLine);
        var users = OtherUsers
                .Where(x => x.IsActive)
                .Append(CurrentUser)
                .ToList();

        CurrentUser.Person.Cash -= amount;
        CurrentUser.History.Add(ActionType.PayMoney, amount);

        if (friend is not null)
        {
            friend.Person.Cash += amount;
            friend.History.Add(ActionType.GetMoney, amount);
        }

        CurrentUser.Person.Assets.Transfer.Delete();

        var notifyAll = users.Select(u => u.Notify(message));
        await Task.WhenAll(notifyAll);
    }
}

public class SendMoneyToWithCredit(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : SendMoneyTo(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message
    {
        get
        {
            var value = CurrentUser.Person.Assets.Transfer.Qtty.AsCurrency();
            var cash = CurrentUser.Person.Cash.AsCurrency();
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
                var delta = CurrentUser.Person.Assets.Transfer.Qtty - CurrentUser.Person.Cash;
                var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;
                CurrentUser.GetCredit(credit);
                await Transfer();

                NextStage = New<Start>();
                return;
        }
    }
}

public class Bankruptcy(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{

}

// ------------------------------------------------

public class BigCircle(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
}

public class AskProfession(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => Terms.Get(28, CurrentUser, "Choose your *profession*");
    public override List<string> Buttons => Professions;

    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[CurrentUser.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, CurrentUser, "Random"))
        .ToList();

    public override IStage NextStage =>
        CurrentUser.Person.Exists
            ? New<SmallCircle>()
            : New<Start>();

    public override Task HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            CurrentUser.Person.Create(profession);
        }

        return Task.CompletedTask;
    }
}

public class ChooseProfession(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[CurrentUser.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, CurrentUser, "Random"))
        .ToList();

    public override Task HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            CurrentUser.Person.Create(profession);
        }

        return Task.CompletedTask;
    }

    public override IStage NextStage =>
        CurrentUser.Person.Exists
            ? New<SmallCircle>()
            : New<Start>();
}

//public class AskLanguage(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
//{
//    public override string Message => "Language/Мова";
//    public override List<string> Buttons => Languages;

//    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

//    public override IStage NextStage() => new ChooseLanguage(Users, User, Terms, Logger);
//}

public class ChooseLanguage(IList<IUser> otherUsers, IUser currentUser, ITermsService termsService, ILogger logger, IAvailableAssets assets) : BaseStage(otherUsers, currentUser, termsService, logger, assets)
{
    public override string Message => "Language/Мова";
    public override List<string> Buttons => Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override IStage NextStage
    {
        get
        {
            if (!CurrentUser.Person.Exists)
            {
                return New<AskProfession>();
            }

            if (CurrentUser.Person.Exists)
            {
                // overwrite profession after changing language
                //user.Person.Profession = Persons.Get(user, user.Person.Profession).Profession;

                if (CurrentUser.Person.Bankruptcy)
                {
                    return New<Bankruptcy>();
                }

                return CurrentUser.Person.Circle == Circle.Big
                    ? New<BigCircle>()
                    : New<SmallCircle>();
            }

            return New<Start>();
        }
    }

    public override Task HandleMessage(string message)
    {
        var language = message.Trim().ToUpper();

        if (Languages.Contains(language))
        {
            CurrentUser.Language = language.ParseEnum<Language>();
        }

        return Task.CompletedTask;
    }
}
