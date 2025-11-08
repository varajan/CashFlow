using CashFlowBot.Data;
using CashFlowBot.Data.Consts;
using CashFlowBot.Data.Users;
using CashFlowBot.Extensions;
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

    IStage HandleMessage(string message);
    IStage NextStage();
    Task SetButtons();
}

public abstract class BaseStage : IStage
{
    public string Name => GetType().Name;
    public IUser User { get; init; }
    public virtual string Message => default;
    public virtual List<string> Buttons => default;
    protected ITermsService Terms { get; }
    protected IButtonsService ButtonsService { get; }

    public BaseStage(IUser user, ITermsService termsService, IButtonsService buttonsService)
    {
        Terms = termsService;
        ButtonsService = buttonsService;
        User = user;
        User.StageName = Name;
    }

    public virtual IStage HandleMessage(string message) => this;
    public virtual IStage NextStage() => this;
    public Task SetButtons() => ButtonsService.SetButtons(this);

    public static IStage GetCurrentStage(IUser user, ITermsService termsService, IButtonsService buttonsService)
    {
        var stage = Type.GetType($"CashFlowBot.Stages.{user.StageName}");
        return stage is not null
            ? (IStage)Activator.CreateInstance(stage, user, termsService, buttonsService)
            : throw new Exception($"{stage} stage not found!");
    }
}

public class Start(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    public override IStage HandleMessage(string _) => new AskLanguage(User, Terms, ButtonsService);
}

public class ShowSmallCircle(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
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

    public override IStage HandleMessage(string _)
    {
        //if (User.Person.ReadyForBigCircle)
        //{
        //    var notifyMessage = Terms.Get(68, user, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", user.Name);

        //    Users.GetActiveUsers(user).Append(user).ForEach(u => bot.SendMessage(u.Id, notifyMessage));
        //}

        User.Person.Assets.CleanUp();
        return new SmallCircle(User, Terms, ButtonsService);
    }
}

public class SmallCircle(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    public override IStage HandleMessage(string message)
    {
        //if (User.Person.ReadyForBigCircle)
        //{
        //    var notifyMessage = Terms.Get(68, user, "{0}'s income is greater, then expenses. {0} is ready for Big Circle.", user.Name);

        //    Users.GetActiveUsers(user).Append(user).ForEach(u => bot.SendMessage(u.Id, notifyMessage));
        //}

        User.Person.Assets.CleanUp();
        return this;
    }

    public override IStage NextStage() => new SmallCircle(User, Terms, ButtonsService);
}

public class BigCircle(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
}

public class AskProfession(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    public override string Message => Terms.Get(28, User, "Choose your *profession*");
    public override List<string> Buttons => Professions;

    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[User.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, User, "Random"))
        .ToList();

    //public override IStage HandleMessage(string _) => new ChooseProfession(User, Terms, ButtonsService);

    public override IStage NextStage() => new ChooseProfession(User, Terms, ButtonsService);

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

public class ChooseProfession(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    private List<string> Professions => Persons.GetAll()
        .Select(x => x.Profession[User.Language])
        .OrderBy(x => x)
        .Append(Terms.Get(139, User, "Random"))
        .ToList();

    public override IStage HandleMessage(string message)
    {
        var profession = Professions.FirstOrDefault(p => p.Equals(message.Trim(), StringComparison.OrdinalIgnoreCase));

        if (profession is not null)
        {
            User.Person.Create(profession);
        }

        return this;
    }

    public override IStage NextStage() =>
        User.Person.Exists
            ? new ShowSmallCircle(User, Terms, ButtonsService)
            : new Start(User, Terms, ButtonsService);
}

public class AskLanguage(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    public override string Message => "Language/Мова";
    public override List<string> Buttons => Languages;

    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override IStage NextStage() => new ChooseLanguage(User, Terms, ButtonsService);
}

public class ChooseLanguage(IUser user, ITermsService termsService, IButtonsService buttonsService) : BaseStage(user, termsService, buttonsService)
{
    private static List<string> Languages => Enum.GetValues<Language>().Select(l => l.ToString()).ToList();

    public override IStage HandleMessage(string message)
    {
        var language = message.Trim().ToUpper();

        if (Languages.Contains(language))
        {
            User.Language = language.ParseEnum<Language>();
        }

        return this;
    }

    public override IStage NextStage()
    {
        if (!User.Person.Exists)
        {
            return new AskProfession(User, Terms, ButtonsService);
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
                ? new BigCircle(User, Terms, ButtonsService)
                : new ShowSmallCircle(User, Terms, ButtonsService);
        }

        return new Start(User, Terms, ButtonsService);
    }
}
