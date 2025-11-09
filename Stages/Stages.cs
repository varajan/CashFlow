using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users;
using CashFlowBot.Data.Users.UserData.PersonData;
using CashFlowBot.Extensions;
using CashFlowBot.Loggers;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowBot.Stages;

public interface IStage
{
    IUser User { get; init; }
    string Name { get; }
    string Message { get; }
    List<string> Buttons { get; }
    IStage NextStage { get; }

    Task HandleMessage(string message);
    Task SetButtons();
}

public abstract class BaseStage : IStage
{
    public string Name => GetType().Name;
    public IUser User { get; init; }
    public IList<IUser> Users { get; init; }
    public virtual string Message => default;
    public virtual List<string> Buttons => default;

    protected IStage nextStage;
    public virtual IStage NextStage { get => this; }

    protected ITermsService Terms { get; }
    protected ILogger Logger { get; }

    public BaseStage(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger)
    {
        Terms = termsService;
        User = user;
        Users = users;
        Logger = logger;
        User.StageName = Name;
    }

    public virtual Task HandleMessage(string message) { return Task.CompletedTask; }
    public Task SetButtons() => User.SetButtons(this);

    public static IStage GetCurrentStage(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger)
    {
        var stage = Type.GetType($"CashFlowBot.Stages.{user.StageName}");
        return stage is not null
            ? (IStage)Activator.CreateInstance(stage, users, user, termsService, logger)
            : throw new Exception($"{stage} stage not found!");
    }

    protected bool MessageEquals(string message, int id, string value) =>
        message.Equals(Terms.Get(id, User, value), StringComparison.InvariantCultureIgnoreCase);
}

public class Start(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override IStage NextStage
    {
        get
        {
            if (User.Person.Exists)
            {
                return User.Person.Circle == Circle.Big
                    ? new BigCircle(Users, User, Terms, Logger)
                    : new SmallCircle(Users, User, Terms, Logger);
            }

            return new ChooseLanguage(Users, User, Terms, Logger);
        }
    }
}

public class SmallCircle(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message => User.Person.Description;
    public override List<string> Buttons
    {
        get
        {
            List<string> buttons = User.History.IsEmpty
                ? [ Terms.Get(31, User, "Show my Data"), Terms.Get(140, User, "Friends") ]
                : [Terms.Get(31, User, "Show my Data"), Terms.Get(140, User, "Friends"), Terms.Get(2, User, "History")];

            buttons.AddRange([Terms.Get(81, User, "Small Opportunity"), Terms.Get(84, User, "Big Opportunity")]);
            buttons.AddRange([Terms.Get(80, User, "Downsize"), Terms.Get(39, User, "Baby")]);
            buttons.AddRange([Terms.Get(79, User, "Pay Check"), Terms.Get(33, User, "Give Money")]);

            if (User.Person.ReadyForBigCircle)
            {
                buttons.Add( Terms.Get(1, User, "Go to Big Circle") );
            }

            return buttons;
        }
    }

    public override IStage NextStage => nextStage ?? new SmallCircle(Users, User, Terms, Logger);

    public async override Task HandleMessage(string message)
    {
        User.Person.Assets.CleanUp();
        if (User.Person.ReadyForBigCircle)
        {
            var notifyMessage = Terms.Get(68, User, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", User.Name);
            Users
                .Where(x => x.Person.Exists && x.IsActive)
                .ForEach(u => u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, 31, "Show my Data"):
                nextStage = new ShowMyData(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 141, "Friends"):
                var users = Users.Where(x => x.IsActive).ToList();
                if (users.Any())
                {
                    nextStage = new Friends(Users, User, Terms, Logger);
                    return;
                }
                else
                {
                    await User.Notify(Terms.Get(141, User, "There are no other players."));
                    return;
                }

            case var m when MessageEquals(m, 2, "History"):
                nextStage = new ShowHistory(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 81, "Small Opportunity"):
                nextStage = new SmallOpportunity(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 84, "Big Opportunity"):
                nextStage = new BigOpportunity(Users, User, Terms, Logger);
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
                nextStage = new SendMoney(Users, User, Terms, Logger);
                return;
        }
    }

    private async Task Downsize()
    {
        var expenses = User.Person.Expenses.Total;
        var info = Terms.Get(87, User, "You were fired. You've payed total amount of your expenses: {0} and lose 2 turns.", expenses.AsCurrency());
        await User.Notify(info);

        if (User.Person.Cash < expenses)
        {
            var delta = expenses - User.Person.Cash;
            var credit = (int)Math.Ceiling(delta / 1_000d) * 1_000;

            User.GetCredit(credit);
            var loanInfo = Terms.Get(88, User, "You've taken {0} from bank.", credit.AsCurrency());
            await User.Notify(loanInfo);
        }

        User.Person.Cash -= expenses;
        User.History.Add(ActionType.Downsize, expenses);
    }

    private async Task Kid()
    {
        if (User.Person.Expenses.Children == 3)
        {
            await User.Notify(Terms.Get(57, User, "You're lucky parent of three children. You don't need one more."));
            return;
        }

        User.Person.Expenses.Children++;
        User.History.Add(ActionType.Child, User.Person.Expenses.Children);

        var termId = User.Person.Expenses.Children == 1 ? 20 : 25;
        var childrenExpenses = User.Person.Expenses.ChildrenExpenses.AsCurrency();
        var count = User.Person.Expenses.Children.ToString();

        await User.Notify(Terms.Get(termId, User, "{0}, you have {1} children expenses and {2} children.", User.Person.Profession, childrenExpenses, count));
    }

    private async Task GetMoney()
    {
        var amount = User.Person.CashFlow;
        User.Person.Bankruptcy = User.Person.Cash + amount < 0;

        if (User.Person.Bankruptcy)
        {
            User.History.Add(ActionType.Bankruptcy);
            nextStage = new Bankruptcy(Users, User, Terms, Logger);
        }

        User.Person.Cash += amount;
        User.History.Add(ActionType.GetMoney, amount);

        await User.Notify(Terms.Get(22, User, "Ok, you've got *{0}*", amount.AsCurrency()));
    }
}

public class ShowMyData(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class Friends(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message
    {
        get
        {
            var message = string.Empty;
            var onSmall = Terms.Get(142, User, "On Small circle:");
            var onBig = Terms.Get(143, User, "On Big circle:");

            var onSmallCircle = Users.Where(x => x.IsActive && x.Person.Circle == Circle.Small).ToList();
            var onBigCircle = Users.Where(x => x.IsActive && x.Person.Circle == Circle.Big).ToList();

            if (onSmallCircle.Any()) message += $"*{onSmall}*\r\n{string.Join("", onSmallCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}\r\n";
            if (onBigCircle.Any()) message += $"*{onBig}* \r\n{string.Join("", onBigCircle.Select(x => $"• {x.Name.Escape()}\r\n"))}";

            return message;
        }
    }

    public override List<string> Buttons => Users.Where(x => x.IsActive).Select(x => x.Name).Append(Terms.Get(6, User, "Cancel")).ToList();

    public async override Task HandleMessage(string message)
    {
        var friend = Users.FirstOrDefault(x => x.Name == message);
        if (friend is null) return;

        await User.Notify(friend.Person.BigCircle ? friend.Person.Description : friend.Description);
        await User.Notify(friend.History.TopFive);
    }
}

public class ShowHistory(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class SmallOpportunity(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class BigOpportunity(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class SendMoney(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{

}

public class Bankruptcy(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

// ------------------------------------------------

public class BigCircle(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class AskProfession(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message => Terms.Get(28, User, "Choose your *profession*");
    public override List<string> Buttons => Professions;

    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[User.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, User, "Random"))
        .ToList();

    public override IStage NextStage =>
        User.Person.Exists
            ? new SmallCircle(Users, User, Terms, Logger)
            : new Start(Users, User, Terms, Logger);

    public override Task HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            User.Person.Create(profession);
        }

        return Task.CompletedTask;
    }
}

public class ChooseProfession(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[User.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, User, "Random"))
        .ToList();

    public override Task HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            User.Person.Create(profession);
        }

        return Task.CompletedTask;
    }

    public override IStage NextStage =>
        User.Person.Exists
            ? new SmallCircle(Users, User, Terms, Logger)
            : new Start(Users, User, Terms, Logger);
}

//public class AskLanguage(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
//{
//    public override string Message => "Language/Мова";
//    public override List<string> Buttons => Languages;

//    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

//    public override IStage NextStage() => new ChooseLanguage(Users, User, Terms, Logger);
//}

public class ChooseLanguage(IList<IUser> users, IUser user, ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message => "Language/Мова";
    public override List<string> Buttons => Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override IStage NextStage
    {
        get
        {
            if (!User.Person.Exists)
            {
                return new AskProfession(Users, User, Terms, Logger);
            }

            if (User.Person.Exists)
            {
                // overwrite profession after changing language
                //user.Person.Profession = Persons.Get(user, user.Person.Profession).Profession;

                if (User.Person.Bankruptcy)
                {
                    return new Bankruptcy(Users, User, Terms, Logger);
                }

                return User.Person.Circle == Circle.Big
                    ? new BigCircle(Users, User, Terms, Logger)
                    : new SmallCircle(Users, User, Terms, Logger);
            }

            return new Start(Users, User, Terms, Logger);
        }
    }

    public override Task HandleMessage(string message)
    {
        var language = message.Trim().ToUpper();

        if (Languages.Contains(language))
        {
            User.Language = language.ParseEnum<Language>();
        }

        return Task.CompletedTask;
    }
}
