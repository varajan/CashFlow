using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users;
using CashFlowBot.Data.Users.UserData.HistoryData;
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

    Task HandleMessage(string message);
    IStage NextStage();
    Task SetButtons();
}

public abstract class BaseStage : IStage
{
    public string Name => GetType().Name;
    public IUser User { get; init; }
    public IUsers Users { get; init; }
    public virtual string Message => default;
    public virtual List<string> Buttons => default;

    protected ITermsService Terms { get; }
    protected ILogger Logger { get; }

    public BaseStage(IUsers users, IUser user,ITermsService termsService, ILogger logger)
    {
        Terms = termsService;
        User = user;
        Users = users;
        Logger = logger;
        User.StageName = Name;
    }

    public virtual Task HandleMessage(string message) { return Task.CompletedTask; }
    public virtual IStage NextStage() => this;
    public Task SetButtons() => User.SetButtons(this);

    public static IStage GetCurrentStage(IUsers users, IUser user, ITermsService termsService, ILogger logger)
    {
        var stage = Type.GetType($"CashFlowBot.Stages.{user.StageName}");
        return stage is not null
            ? (IStage)Activator.CreateInstance(stage, users, user, termsService, logger)
            : throw new Exception($"{stage} stage not found!");
    }

    protected bool MessageEquals(string message, int id, string value) =>
        message.Equals(Terms.Get(id, User, value), StringComparison.InvariantCultureIgnoreCase);
}

public class Start(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    private IStage nextStage;

    public override IStage NextStage() => nextStage ?? new ChooseLanguage(Users, User, Terms, Logger);
}

public class SmallCircle(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
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

    private IStage nextStage;

    public async override Task HandleMessage(string message)
    {
        User.Person.Assets.CleanUp();
        if (User.Person.ReadyForBigCircle)
        {
            var notifyMessage = Terms.Get(68, User, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", User.Name);
            Users.GetActiveUsers(User).ForEach(u => u.Notify(notifyMessage));
        }

        switch (message)
        {
            case var m when MessageEquals(m, 31, "Show my Data"):
                nextStage = new ShowMyData(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 141, "Friends"):
                nextStage = new Friends(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 2, "History"):
                nextStage = new History(Users, User, Terms, Logger);
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
                nextStage = new GetMoney(Users, User, Terms, Logger);
                return;

            case var m when MessageEquals(m, 33, "Give Money"):
                nextStage = new SendMoney(Users, User, Terms, Logger);
                return;
        }
    }

    public override IStage NextStage() => nextStage ?? new SmallCircle(Users, User, Terms, Logger);

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
}

public class ShowMyData(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class Friends(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class History(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class SmallOpportunity(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class BigOpportunity(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class GetMoney(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class SendMoney(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class BigCircle(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
}

public class AskProfession(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message => Terms.Get(28, User, "Choose your *profession*");
    public override List<string> Buttons => Professions;

    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[User.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, User, "Random"))
        .ToList();

    //public async override Task HandleMessage(string _) => new ChooseProfession(Users, User, Terms, ButtonsService, Logger);


    public override Task HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            User.Person.Create(profession);
        }

        return Task.CompletedTask;
    }

    public override IStage NextStage() =>
        User.Person.Exists
            ? new SmallCircle(Users, User, Terms, Logger)
            : new Start(Users, User, Terms, Logger);

    //public override IStage NextStage() => new ChooseProfession(Users, User, Terms, Logger);

    //await bot.SendTextMessageAsync(user.Id, Terms.Get(28, user, "Choose your *profession*"),
    //        replyMarkup: rkm, parseMode: ParseMode.Markdown);

    // while (professions.Any())
    //{
    //    var x = professions.Take(3).ToList();
    //    professions = professions.Skip(3).ToList();

    //    if (x.Count == 3) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1], x[2]]); continue; }
    //    if (x.Count == 2) { rkm.Keyboard = rkm.Keyboard.Append([x[0], x[1]]); continue; }
    //    if (x.Count == 1) { rkm.Keyboard = rkm.Keyboard.Append([x[0]]); }
    //}
}

public class ChooseProfession(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
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

    public override IStage NextStage() =>
        User.Person.Exists
            ? new SmallCircle(Users, User, Terms, Logger)
            : new Start(Users, User, Terms, Logger);
}

//public class AskLanguage(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
//{
//    public override string Message => "Language/Мова";
//    public override List<string> Buttons => Languages;

//    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

//    public override IStage NextStage() => new ChooseLanguage(Users, User, Terms, Logger);
//}

public class ChooseLanguage(IUsers users, IUser user,ITermsService termsService, ILogger logger) : BaseStage(users, user, termsService, logger)
{
    public override string Message => "Language/Мова";
    public override List<string> Buttons => Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public async override Task HandleMessage(string message)
    {
        var language = message.Trim().ToUpper();

        if (Languages.Contains(language))
        {
            User.Language = language.ParseEnum<Language>();
        }
    }

    public override IStage NextStage()
    {
        if (!User.Person.Exists)
        {
            return new AskProfession(Users, User, Terms, Logger);
        }

        if (User.Person.Exists)
        {
            // overwrite profession after changing language
            //user.Person.Profession = Persons.Get(user, user.Person.Profession).Profession;

            //if (user.Person.Bankruptcy)
            //{
            //    BankruptcyActions.ShowMenu(bot, user);
            //    return;
            //}

            return User.Person.Circle == Circle.Big
                ? new BigCircle(Users, User, Terms, Logger)
                : new SmallCircle(Users, User, Terms, Logger);
        }

        return new Start(Users, User, Terms, Logger);
    }
}
